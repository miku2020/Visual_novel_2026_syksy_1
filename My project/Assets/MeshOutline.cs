using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshOutline : MonoBehaviour
{
    const string ShellName = "Outline Shell (generated)";

    [Tooltip("Aseta tähän Outline Shell -materiaali. Älä vaihda objektin MeshRendererin pintamateriaalia.")]
    public Material outlineMaterial;

    Mesh _sourceMesh;
    Mesh _outlineMesh;

    // Päivitä kuori, kun komponentti otetaan käyttöön tai sen asetuksia muutetaan.
    void OnEnable() => UpdateShell();

    void OnValidate() => UpdateShell();

    void OnDisable()
    {
        Transform shell = transform.Find(ShellName);

        // Poista vain tämän komponentin luoma kuori.
        if (shell != null)
        {
            if (Application.isPlaying) Destroy(shell.gameObject);
            else DestroyImmediate(shell.gameObject);
        }

        DestroyOutlineMesh();
    }

    void UpdateShell()
    {
        if (!isActiveAndEnabled) return;

        var sourceFilter = GetComponent<MeshFilter>();
        var sourceRenderer = GetComponent<MeshRenderer>();
        if (sourceRenderer.sharedMaterial != null &&
            sourceRenderer.sharedMaterial.shader != null &&
            sourceRenderer.sharedMaterial.shader.name == "Custom/Outline Shell")
        {
            Debug.LogWarning(
                "Outline Shell -materiaali on objektin pintamateriaalina. Palauta alkuperäinen pintamateriaali MeshRendereriin ja aseta Outline Shell vain MeshOutline-komponentin Outline Material -kenttään.",
                this);
        }

        Transform shellTransform = transform.Find(ShellName);
        if (shellTransform == null)
        {
            // Erillinen lapsi renderöidään alkuperäisen mesh-pinnan päälle.
            var shellObject = new GameObject(ShellName);
            shellObject.transform.SetParent(transform, false);
            shellTransform = shellObject.transform;
        }

        Mesh sourceMesh = sourceFilter.sharedMesh;
        if (_sourceMesh != sourceMesh)
        {
            DestroyOutlineMesh();
            _sourceMesh = sourceMesh;

            if (sourceMesh != null)
            {
                // Oma kopio sallii reunuksen normaalien pehmentämisen muuttamatta alkuperäistä meshiä.
                _outlineMesh = Instantiate(sourceMesh);
                _outlineMesh.name = sourceMesh.name + " (Outline)";
                SmoothNormals(_outlineMesh);
            }
        }

        shellTransform.localPosition = Vector3.zero;
        shellTransform.localRotation = Quaternion.identity;
        shellTransform.localScale = Vector3.one;

        var shellFilter = shellTransform.GetComponent<MeshFilter>();
        if (shellFilter == null) shellFilter = shellTransform.gameObject.AddComponent<MeshFilter>();
        // Reunuksen muoto tulee tämän objektin MeshFilterin meshistä.
        shellFilter.sharedMesh = _outlineMesh;

        var shellRenderer = shellTransform.GetComponent<MeshRenderer>();
        if (shellRenderer == null) shellRenderer = shellTransform.gameObject.AddComponent<MeshRenderer>();
        shellRenderer.sharedMaterial = outlineMaterial;
        shellRenderer.shadowCastingMode = ShadowCastingMode.Off;
        shellRenderer.receiveShadows = false;
        // Säilytä päällekkäinen kuori ja piirrä se alkuperäisen pinnan jälkeen.
        shellRenderer.allowOcclusionWhenDynamic = false;
        // Näytä kuori vasta, kun sekä lähdemesh että reunusmateriaali on asetettu.
        shellRenderer.enabled = _outlineMesh != null && outlineMaterial != null;
    }

    static void SmoothNormals(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        if (normals.Length != vertices.Length)
        {
            mesh.RecalculateNormals();
            normals = mesh.normals;
        }

        var normalSums = new Dictionary<Vector3, Vector3>(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            if (normalSums.TryGetValue(vertices[i], out Vector3 sum))
                normalSums[vertices[i]] = sum + normals[i];
            else
                normalSums.Add(vertices[i], normals[i]);
        }

        for (int i = 0; i < vertices.Length; i++)
            normals[i] = normalSums[vertices[i]].normalized;

        mesh.normals = normals;
    }

    void DestroyOutlineMesh()
    {
        if (_outlineMesh == null) return;

        Transform shell = transform.Find(ShellName);
        if (shell != null)
        {
            var shellFilter = shell.GetComponent<MeshFilter>();
            if (shellFilter != null && shellFilter.sharedMesh == _outlineMesh)
                shellFilter.sharedMesh = null;
        }

        if (Application.isPlaying) Destroy(_outlineMesh);
        else DestroyImmediate(_outlineMesh);

        _outlineMesh = null;
    }
}