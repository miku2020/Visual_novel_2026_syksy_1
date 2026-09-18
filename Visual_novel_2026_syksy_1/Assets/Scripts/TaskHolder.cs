using System;
using UnityEngine;

/*
=== README ===

TaskHolder valitsee tehtävän tilan perusteella avattavan huonevariantin.

Inspector-asetukset:
- Task Name: tehtävän nimi.
- Task Underway Room Index: huone, joka avataan tehtävän ollessa kesken.
- Completed Task Room Index: huone, joka avataan tehtävän valmistuttua.
- Is Completed: tehtävän alkutila.

Huoneindeksit ovat nollapohjaisia. BootstrapSceneManagerin roomScenes-listan
ensimmainen huone on indeksi 0.

Esimerkki:
- tehtava 0
- Task Underway Room Index = 1
- Completed Task Room Index = 2
- Is Completed = false

Metodit:
- OpenRoomForTask(0): avaa tehtavan nykyisen tilan mukaisen huoneen.
- CompleteTask(0): merkitsee tehtavan valmiiksi.
- SetTaskCompleted(0, false): merkitsee tehtavan kesken olevaksi.
- IsTaskCompleted(0): palauttaa, onko tehtava valmis.

Esimerkki tehtavan suorittamisen jalkeen:
taskHolder.CompleteTask(0);
taskHolder.OpenRoomForTask(0);

TaskHolder ei tallenna tilaa levylle. Jos tilan pitaa sailyä pelin
sulkemisen jalkeen, se taytyy tallentaa erikseen pelin tallennusjarjestelmaan.
=== END README ===
*/
public class TaskHolder : MonoBehaviour
{
    [Serializable]
    private class TaskRoute
    {
        [SerializeField] private string taskName;
        [SerializeField] private int taskUnderwayRoomIndex = -1;
        [SerializeField] private int completedTaskRoomIndex = -1;
        [SerializeField] private bool isCompleted;

        public bool IsCompleted => isCompleted;
        public int RoomIndex => isCompleted ? completedTaskRoomIndex : taskUnderwayRoomIndex;

        public void SetCompleted(bool completed)
        {
            isCompleted = completed;
        }
    }

    [SerializeField] private TaskRoute[] tasks;

    public void CompleteTask(int taskIndex)
    {
        SetTaskCompleted(taskIndex, true);
    }

    public void SetTaskCompleted(int taskIndex, bool completed)
    {
        if (!TryGetTask(taskIndex, out TaskRoute task))
            return;

        task.SetCompleted(completed);
    }

    public void OpenRoomForTask(int taskIndex)
    {
        if (!TryGetTask(taskIndex, out TaskRoute task))
            return;

        if (task.RoomIndex < 0)
        {
            Debug.LogError($"Task {taskIndex} does not have a valid room variant configured.");
            return;
        }

        if (BootstrapSceneManager.Instance == null)
        {
            Debug.LogError("BootstrapSceneManager is not available.");
            return;
        }

        BootstrapSceneManager.Instance.LoadRoom(task.RoomIndex);
    }

    public bool IsTaskCompleted(int taskIndex)
    {
        return TryGetTask(taskIndex, out TaskRoute task) && task.IsCompleted;
    }

    private bool TryGetTask(int taskIndex, out TaskRoute task)
    {
        task = null;

        if (tasks == null || taskIndex < 0 || taskIndex >= tasks.Length)
        {
            Debug.LogError($"Task index {taskIndex} is outside the configured task list.");
            return false;
        }

        task = tasks[taskIndex];
        if (task == null)
        {
            Debug.LogError($"Task {taskIndex} is not configured.");
            return false;
        }

        return true;
    }
}
