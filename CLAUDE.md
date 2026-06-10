# Blacksmith Simulator — Claude Çalışma Notları

Gerçekçi demir dövme odaklı VR mekaniği prototipi (Unity 6, HDRP, C#).
Portfolio/showcase kapsamı, ~6 hafta, solo geliştirici.

## Her oturumda önce bunları oku (kaynak gerçekler)

Bu `.md` dosyaları projenin hafızasıdır (hepsi `Assets/Documantations/`
altında); kod yazmadan önce ilgili olanları oku:

- `Assets/Documantations/proje-talimatlari.md` — rol, çalışma tarzı, kapsam kuralları
- `Assets/Documantations/kararlar.md` — verilmiş tüm mimari kararlar + gerekçeleri (kilitli sözleşmeler)
- `Assets/Documantations/teknik-stack.md` — araçlar, sürümler, klasör yapısı, referanslar
- `Assets/Documantations/yol-haritasi.md` — milestone'lar ve görev ilerlemesi (resume sinyali)
- `Assets/Documantations/hafta1-oturum-plani.md` — Hafta 1'in token-bütçeli oturum bölümlemesi (tamamlandı)
- `Assets/Documantations/hafta2-oturum-plani.md` — Hafta 2'nin (VR) token-bütçeli oturum bölümlemesi

Bir karar değişirse ilgili `.md` güncellenir.

## Oturum protokolü (ÖNEMLİ)

Her hafta, kullanım limitine (5 saat/genel) görev ortasında takılmamak için
**token-bütçeli oturumlara** bölündü. Kullanıcı *"Hafta H Oturum N'e başlıyoruz"*
dediğinde:

1. O haftanın plan dosyasını aç (`Assets/Documantations/haftaH-oturum-plani.md`),
   Oturum N'in kapsamını oku.
2. `Assets/Documantations/yol-haritasi.md`'den nerede kalındığını ve
   `Assets/Documantations/kararlar.md`'den ilgili kilitli kararları doğrula.
3. Sadece **o oturumun kapsamını** yap. Erken biterse sonrakine geçme — bilinçli
   dur, limit penceresini koru.
4. Oturum bitince `yol-haritasi.md`'deki ilgili görevleri `[x]` işaretle
   ("— Oturum N" notuyla). Bu, sonraki pencerenin resume sinyalidir.
5. **Plan dosyasında 🔴/izole işaretli ağır oturumlar asla birleştirilmez**
   (her hafta planı bunları kendi içinde işaretler). Sabit/bilinen veri (ör.
   TriTable) ve hazır asset'ler sıfırdan üretilmez; güvenilir referanstan alınır.

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
  List<Vector3> normals, List<int> triangles)`, vertex'ler chunk-local. Cube-index
  biti solid köşede (density ≥ iso) set edilir → Unity winding'inde dışa-bakan
  yüzler. Normaller yoğunluk gradyanından (merkezi fark, `-gradient`) gelir;
  welding yok, chunk sınırları dikişsiz (smooth — Oturum 6).
- `MarchingCubes/MarchingCubesTables` — EdgeTable + TriTable + CornerOffsets (8) +
  EdgeConnections (12) sabitleri (Paul Bourke konvansiyonu).
- `ForgeBlock` (facade, tek MonoBehaviour) — hepsini bağlar + mouse/raycast input
  (eski/yeni Input System ikisinde de çalışır, `#if ENABLE_INPUT_SYSTEM`),
  world↔local transform (`InverseTransformPoint`). Tüm input için tek giriş:
  `DeformAt(worldPoint)` — Hafta 2'de VR çekiç aynı metodu çağıracak.

Blok: 32×16×32 hücre (= 33×17×33 nokta) = 4×2×4 chunk. Chunk overlap: her chunk
8 hücre işler, 9 nokta okur; sınır noktası komşuyla paylaşılır (dikiş önleme).

**Hafta 1 durumu:** Çekirdek tamam (Oturum 1–6). Mouse ile deformasyon çalışıyor,
smooth normaller dikişsiz, profiler'da ForgeBlock.Update ~0 ms / 0 B GC. Sıradaki:
Hafta 2 (VR entegrasyonu).
