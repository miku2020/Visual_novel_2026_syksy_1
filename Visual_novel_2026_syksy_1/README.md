# Visual novel 2026

## Details-ikkunan käyttöönotto

Details-ikkuna on oma Unity Scene, joka ladataan BootstrapSceneManagerin kautta Addressables-järjestelmällä additive-tilassa.

### 1. Luo Details Scene

1. Luo uusi Scene nimellä esimerkiksi `Details`.
2. Lisää Sceneen ikkunaobjekti, jonka juurena on UI `Canvas`.
3. Lisää ikkunan juureen `DetailsWindowController`.
4. Lisää controllerille UI-paneeli ja nimeä se `Details`, jos `detailsWindow`-kenttää ei aseteta Inspectorissa.
5. Lisää Details-ikkunaan 15 korttia sisältävä UI-objekti.
6. Lisää korttien säiliöön `GridLayoutGroup` ja aseta se kolmeen sarakkeeseen.

Viisitoista korttia muodostaa tällöin 3x5-gridin.

### 2. Lisää Scene Addressables-järjestelmään

1. Avaa Unityn Addressables Groups -ikkuna.
2. Lisää Details Scene Addressable-ryhmään.
3. Varmista, että Scene saa validin Addressables-osoitteen.
4. Valitse Bootstrap Scene -objekti.
5. Vedä Details Scene `Details Window Scene` -kenttään.

### 3. Kytke avauspainike

Painikkeen tarvitsee kutsua vain yhtä metodia:

`BootstrapSceneManager.OpenDetailWindow()`

Älä lisää painikkeeseen erillistä `DetailsWindowController.OpenDetails()`-kutsua. Bootstrap lataa Details-scenen, ja scenen oma controller käynnistää sivulta liukuvan animaation automaattisesti.

### 4. Määritä 15 korttia

Jokaisella `Detail Card` -kortilla on omat kentät:

- `Title`: kortin otsikko.
- `Title Text`: kortin otsikon TMP-komponentti.
- `Dropdown`: kortin oma `TMP_Dropdown`.
- `Options`: kyseisen kortin omat vaihtoehdot.

Korttien vaihtoehdot eivät vaikuta toisiinsa. Esimerkiksi yhdellä kortilla voi olla:

- `Musta`
- `Puna`
- `Vihree`

Ja toisella:

- `Musta`
- `Crimson-red`
- `Dark-electric-blue`
- `Forest-green`

### 5. Sulkeminen

Details Scene suljetaan ensimmäisellä `Esc`- tai `Backspace`-painalluksella. Sen jälkeen seuraava painallus sulkee aktiivisen huoneen `RoomManager`in kautta.

Sulkeminen käyttää BootstrapSceneManagerin metodia:

`BootstrapSceneManager.CloseDetailWindow()`

### Tarkistuslista

- [ ] Details Scene on lisätty Addressables-järjestelmään.
- [ ] Details Scene on asetettu Bootstrapin `Details Window Scene` -kenttään.
- [ ] Button kutsuu vain `OpenDetailWindow()`-metodia.
- [ ] Details Scene sisältää `DetailsWindowController`-komponentin.
- [ ] Gridissä on 15 korttia ja kolme saraketta.
- [ ] Jokaisella kortilla on oma TMP-otsikko ja TMP-dropdown.
- [ ] Jokaisen kortin vaihtoehdot on täytetty Inspectorissa.
