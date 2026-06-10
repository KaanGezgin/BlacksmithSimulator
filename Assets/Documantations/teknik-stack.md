# Teknik Stack — Blacksmith Simulator

Bu dosya projede kullanılan tüm teknik araçları, sürümleri ve süreçleri tutar.
Stack değiştiğinde güncellenip yeniden yüklenmelidir.

---

## Motor

- **Unity 6** (en yeni sürüm)
- **Render Pipeline:** HDRP (High Definition Render Pipeline)

---

## VR Eklentileri

- **XR Interaction Toolkit** (Unity resmi)
- **OpenXR Plugin** (Unity resmi — SteamVR/Oculus/diğer cihazlar için)
- **Auto Hand** — Fiziksel el etkileşimleri (çekiç tutma, kavrama)

---

## Programlama

- **Dil:** C#
- **Hedeflenen ileri optimizasyonlar:**
  - Unity Job System (NativeArray uyumlu mimari mevcut)
  - Burst Compiler (gerektiğinde aktive edilecek)
  - Mathematics paketi (float3, vb. Job uyumlu tipler)

---

## 3B Modelleme ve Asset Araçları

- **Strateji:** 3B modelleme sıfırdan yapılmayacak. Asset'ler hazır mağazalardan tedarik edilecek:
  - **Unity Asset Store** (birincil kaynak)
  - **Fab** (Epic'in birleşik mağazası — Quixel Megascans dahil)
  - Diğer güvenilir mağazalar (gerektiğinde)
- Bu yaklaşım solo + portfolio kapsamı ile uyumlu: süre asıl mekanik (deformasyon, VR etkileşim) üzerinde harcanacak, modelleme dışarıdan
- **HDRI / çevre aydınlatması:** Polyhaven (ücretsiz, kaliteli)

---

## Ses

- **Şu an belirlenmedi.** Hafta 4 (Ortam, Işıklandırma, Ses) yaklaştığında en uygun çözüm araştırılıp seçilecek.
- Karar verilmesi gereken alanlar:
  - Ses motoru (Unity native AudioSource yeterli mi, FMOD veya Wwise gerekli mi?)
  - Ses kaynağı (asset mağazaları, ücretsiz ses kütüphaneleri, kayıt)
- Kritik ses gereksinimleri (ne seçilirse seçilsin desteklenmeli):
  - Çekiç sesinin sıcaklığa göre dinamik değişimi
  - Ortam sesleri (ocak, körük, söndürme suyu cızırtısı)
  - Mekânsal ses (VR için 3D positional audio)

---

## UI

- **Diegetic UI yaklaşımı** (dünya içi, floating UI minimum)
- Unity UI Toolkit (yeni) veya UGUI (klasik) — gerektiğinde karar verilecek
- Dünya içi nesneler: Sipariş tabelası, sıcaklık göstergesi (örs üzerinde termometre vb.)

---

## Versiyon Kontrol

- **Git + GitHub** (proje portfolio amaçlı, public repo)
- **Git LFS** — Büyük asset'ler için (zorunlu, Unity projelerinde standart)
- `.gitignore` Unity için standart şablon kullanılacak

---

## Build / Deploy

- Build hedefi: Windows x64 (PCVR)
- Dağıtım: GitHub releases (portfolio sergisi için)
- Steam release planı: Yok (portfolio kapsamında)

---

## Klasör Yapısı (Projede)

```
Assets/
├── Scripts/
│   └── Forge/
│       ├── Core/                  (VoxelData, Chunk, ChunkManager)
│       ├── MarchingCubes/         (Generator, Tables)
│       └── ForgeBlock.cs          (ana facade)
├── Materials/
├── Prefabs/
├── Scenes/
└── Art/
    ├── Models/
    ├── Textures/
    └── Audio/
```

---

## Dış Kaynaklar / Referanslar

- **Marching Cubes lookup tabloları:** Paul Bourke (paulbourke.net/geometry/polygonise) veya Sebastian Lague'in MIT lisanslı GitHub repo'su
- **VR fiziksel etkileşim referansı:** Blade & Sorcery, Forge VR

---
