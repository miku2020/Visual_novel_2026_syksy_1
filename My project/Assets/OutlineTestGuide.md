# Outline-testin käyttöönotto

Tämä testi piirtää objektin alkuperäisen materiaalin taakse laajennetun, yksivärisen mesh-kuoren. Reunuksen muoto kopioidaan objektin `MeshFilter`-komponentin meshistä. Reunus käyttää erillistä mesh-kopiota, jonka saman kärkipisteen normaalit yhdistetään; tämä auttaa myös teräväreunaisissa mesheissä, kuten Unityn peruskuutiossa. Shader laajentaa kopion verteksiä normaalin suuntaan, ja etupintojen poisto jättää näkyviin kuoren ulkoreunan.

## Käyttöönotto

1. Valitse Scene-näkymästä 3D-objekti, jolla on `MeshFilter` ja `MeshRenderer`.
2. Luo projektiin uusi materiaali valitsemalla **Assets → Create → Material**.
3. Aseta materiaalin shaderiksi **Custom → Outline Shell**.
4. Säädä materiaalin **Outline Width** -arvoa. Aloita arvosta `0.03`; paksuus on paikallisissa mesh-yksiköissä ja riippuu objektin koosta. Unityn peruskuutiolle `0.3` on yleensä hyvin paksu.
5. Lisää objektille `MeshOutline`-komponentti ja vedä luotu materiaali komponentin **Outline Material** -kenttään.
6. Jätä objektin `MeshRenderer`-komponenttiin sen alkuperäinen pintamateriaali. Älä aseta `Outline Shell` -materiaalia objektin pintamateriaaliksi, koska shader piirtää vain reunuskuoren ja objekti näyttää silloin näkymättömältä.
7. Tarkista tulos Scene- tai Game-näkymässä.

Komponentti luo alkuperäisen objektin alle lapsiobjektin nimeltä `Outline Shell (generated)` ja asettaa sille saman meshin sekä reunusmateriaalin. Lapsiobjekti poistetaan, kun `MeshOutline` poistetaan käytöstä.

## Jos reunus ei näy

- Varmista, että reunusmateriaali on asetettu `Outline Material` -kenttään.
- Tarkista, että objektilla on aktiivinen `MeshFilter` ja että sen **Mesh**-kentässä on mesh.
- Käytä URP-renderöintiputkea ja varmista, että shaderissa näkyy **Custom → Outline Shell**.
- Säädä `Outline Width` -arvoa: liian pieni arvo voi olla vaikea nähdä, ja liian suuri arvo vääristää reunusta.
- Tämä toteutus käyttää `MeshFilter`-komponenttia; se ei suoraan tue `SkinnedMeshRenderer`-animaatiomeshin reunustamista.