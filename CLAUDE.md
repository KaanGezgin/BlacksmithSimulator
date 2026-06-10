# Blacksmith Simulator — Claude Çalışma Notları

Gerçekçi demir dövme odaklı VR mekaniği prototipi (Unity 6, HDRP, C#).
Portfolio/showcase kapsamı, ~6 hafta, solo geliştirici.

## Her oturumda önce bunları oku (kaynak gerçekler)

Bu `.md` dosyaları projenin hafızasıdır; kod yazmadan önce ilgili olanları oku:

- `Assets/proje-talimatlari.md` — rol, çalışma tarzı, kapsam kuralları
- `Assets/kararlar.md` — verilmiş tüm mimari kararlar + gerekçeleri (kilitli sözleşmeler)
- `Assets/teknik-stack.md` — araçlar, sürümler, klasör yapısı, referanslar
- `Assets/yol-haritasi.md` — milestone'lar ve görev ilerlemesi (resume sinyali)
- `Assets/hafta1-oturum-plani.md` — Hafta 1'in token-bütçeli oturum bölümlemesi

Bir karar değişirse ilgili `.md` güncellenir.

## Oturum protokolü (ÖNEMLİ)

Hafta 1, kullanım limitine (5 saat/genel) görev ortasında takılmamak için
**token-bütçeli oturumlara** bölündü. Kullanıcı *"Oturum N'e başlıyoruz"*
dediğinde:

1. `Assets/hafta1-oturum-plani.md`'yi aç, Oturum N'in kapsamını oku.
2. `Assets/yol-haritasi.md`'den nerede kalındığını ve `Assets/kararlar.md`'den
   ilgili kilitli kararları doğrula.
3. Sadece **o oturumun kapsamını** yap. Erken biterse sonrakine geçme — bilinçli
   dur, limit penceresini koru.
4. Oturum bitince `yol-haritasi.md`'deki ilgili görevleri `[x]` işaretle
   ("— Oturum N" notuyla). Bu, sonraki pencerenin resume sinyalidir.
5. **Oturum 3 (MC tabloları) ve Oturum 4 (generator) asla birleştirilmez** —
   ikisi de ağır. TriTable sabittir; sıfırdan üretme, güvenilir referanstan
   (Paul Bourke / Sebastian Lague MIT) al.

## Çalışma tarzı

- İletişim **Türkçe**. Kod içi yorum ve debug satırları **İngilizce**.
- Kod örneklerinde **önce mimariyi tartış, sonra kod yaz** (teknik danışman tonu).
- Scope creep'e karşı uyar; prototip/portfolio bağlamını koru.
- Kod modüler, yorumlu, okunabilir olsun — kullanıcı inceleyip Unity'ye uyarlar.

## Forge kodu mimari özeti (Hafta 1)

`Assets/Scripts/Forge/` altında, namespace `Forge.Core` / `Forge.MarchingCubes`:

- `Core/VoxelData` — yoğunluk alanı + deformasyon matematiği. Saf veri, chunk'tan
  habersiz. Flat `float[]` (NativeArray-layout uyumlu). ISO 0.5, denting-only.
- `Core/Chunk` — 8³ bölgenin GameObject + Mesh + MeshCollider sarmalayıcısı
  (plain class). Chunk-local vertex.
- `Core/ChunkManager` — Dictionary chunk deposu, dirty-tracking, overlap→dirty
  eşleme, regen orkestrasyonu. ChunkSize = 8 hücre.
- `MarchingCubes/MarchingCubesGenerator` — chunk yoğunluğundan mesh üretir.
  Sözleşme: `Generate(VoxelData, chunkCoord, chunkSize, List<Vector3> vertices,
  List<int> triangles)`, vertex'ler chunk-local.
- `MarchingCubes/MarchingCubesTables` — EdgeTable + TriTable sabitleri.
- `ForgeBlock` (facade, tek MonoBehaviour) — hepsini bağlar + mouse/raycast input,
  world↔local transform.

Blok: 32×16×32 hücre (= 33×17×33 nokta) = 4×2×4 chunk. Chunk overlap: her chunk
8 hücre işler, 9 nokta okur; sınır noktası komşuyla paylaşılır (dikiş önleme).
