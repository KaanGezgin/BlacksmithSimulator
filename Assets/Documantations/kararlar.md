# Kararlar — Blacksmith Simulator

Bu dosya proje süresince verilmiş tüm önemli kararları ve gerekçelerini tutar.
Yeni bir karar verildiğinde bu dosya güncellenir ve projeye yeniden yüklenir.

---

## Motor Seçimi

**Karar:** Unity 6
**Tarih:** Proje başlangıcı

**Gerekçe:**
- C# ile özel mesh deformasyon algoritmaları yazmak C++'a göre daha hızlı iterasyon sağlıyor
- Demir dövme simülasyonunun kalbi olan deformasyon sistemi sıfırdan yazılacak; motorun hazır özelliklerinden çok kendi kodun esnekliği kritik
- VR ekosistemi Unity'de tarihsel olarak daha olgun (Blade & Sorcery, Forge VR gibi referanslar)
- Asset Store'da blacksmithing ile ilgili daha fazla hazır kaynak bulunabiliyor
- Kullanıcının önceki Unity deneyimi mevcut

**Reddedilen Alternatif:** Unreal Engine 5
- Lumen ile sıcak metal emisyonu kutudan çıkar çıkmaz daha etkileyici
- Niagara parçacık sistemi kıvılcım simülasyonunda bir adım önde
- Ancak özel deformasyon sistemi yazma esnekliği Unity'de daha yüksek
- C++ iterasyon hızı C#'a göre daha yavaş

---

## Render Pipeline

**Karar:** HDRP (High Definition Render Pipeline)
**Tarih:** Proje başlangıcı

**Gerekçe:**
- PCVR hedefi ile performans bütçesi yeterli
- Sıcak metalin emisyonu, volumetric duman, karanlık demirhanedeki dramatik ışıklandırma HDRP'de görsel olarak çok daha güçlü
- Portfolio projesi olduğu için "wow faktörü" ve ilk izlenim kritik
- HDRP fotorealistik sonucu kutudan çıkar çıkmaz veriyor

**Reddedilen Alternatif:** URP
- Standalone VR (Quest) hedeflenseydi zorunlu olurdu
- PCVR hedefinde HDRP'nin görsel kazanımları URP'nin performans avantajına ağır basıyor

---

## Hedef Platform

**Karar:** PCVR (Steam / SteamVR / OpenXR)
**Tarih:** Proje başlangıcı

**Gerekçe:**
- Tam görsel kalite ve karmaşık simülasyon mümkün
- Performans bütçesi geniş, optimizasyon ikincil öncelik
- Steam üzerinden showcase yapmak portfolio için yeterli erişim sağlıyor

**Reddedilen Alternatifler:**
- **Quest standalone:** Mobil donanım performans bütçesini çok daraltırdı; voxel deformasyon ve dinamik ışıklandırma için ciddi taviz gerekirdi
- **Hibrit (PC + Quest):** İki ayrı optimizasyon profili portfolio kapsamı için fazla yük

---

## VR Framework

**Karar:** XR Interaction Toolkit (native grab — `XRGrabInteractable` + Velocity Tracking)
**Tarih:** Proje başlangıcı (Auto Hand kararı **2026-06-10**'da revize edildi)

**Gerekçe:**
- XRI: Unity'nin resmi çözümü, OpenXR üzerinden tüm cihazları destekler, sürekli güncel, **ücretsiz**
- Dövme mekaniği için gereken tek şey çekici tutup **fiziksel hızla** bloğa vurmak. XRI'nin Velocity Tracking grab'i tutulan nesneyi fizikle hareket ettirir → çarpışma hızı (`relativeVelocity`) ölçülüp `ForgeBlock.DeformAt()` üzerinden deformasyona haritalanır. Mekaniğin tam karşılığı.
- Eller fiziksel parmak etkileşimi değil, basit **görsel** (controller/el modeli) olarak yeterli.

**Reddedilen Alternatif 1:** Auto Hand (önceki karar — **2026-06-10'da düşürüldü**)
- Fiziksel el (parmak sarma/itme) güçlü ama dövme için gereksiz; asıl ihtiyaç çekiç-impact, onu XRI zaten karşılıyor
- ~$90 maliyet, solo/portfolio bütçesi için gereksiz harcama
- İleride maşayla küçük parça tutma gibi fiziksel-el ihtiyacı doğarsa yeniden değerlendirilir (ücretsiz/açık-kaynak öncelik)

**Reddedilen Alternatif 2:** VRIF / Hurricane VR
- Daha kapsamlı ama lisanslı/ücretli; XRI ücretsiz ve prototip için yeterli

---

## Deformasyon Sistemi Yaklaşımı

**Karar:** Marching Cubes + SDF (Signed Distance Field) tabanlı
**Tarih:** Proje başlangıcı

**Gerekçe:**
- Voxel'den (Minecraft tarzı küpler) çok daha pürüzsüz, organik yüzey üretir
- Vertex displacement'tan çok daha esnek, karmaşık şekiller mümkün
- Demir dövmenin doğası gereği organik deformasyon gerektiriyor
- PCVR performans bütçesi karmaşıklığı kaldırabiliyor
- Portfolio için "wow" faktörü smooth deformasyonda daha yüksek

**Reddedilen Alternatifler:**
- **Vertex displacement:** Hızlı ama detay sınırlı, karmaşık şekiller zor
- **Voxel-based (küp tarzı):** Esnek ama görsel olarak demir dövmeye uygun değil

---

## Mimari Kararlar (Hafta 1)

**Karar:** Chunk tabanlı sistem, NativeArray + Job System uyumlu
**Tarih:** Hafta 1 planlaması

**Detaylar:**
- **Chunk boyutu:** 8³ voxel
- **Başlangıç blok boyutu:** 32×16×32 voxel (yerleştirme alanı)
- **ISO threshold:** 0.5 (yoğunluk konvansiyonu: 1.0 = dolu metal, 0.0 = hava)
- **Chunk overlap:** Her chunk komşusundan 1 voxel okur (sınır dikişlerini önlemek için)
- **Dictionary tabanlı chunk depolama:** İleride genişleme esnekliği için (sabit grid yerine)
- **Her chunk için ayrı GameObject + MeshCollider:** Unity'nin frustum culling sistemini kullanmak ve raycast'leri mümkün kılmak için

**Gerekçe:**
- Tüm bloğu her darbede yeniden hesaplamak israf — chunk sistemi sadece etkilenen bölgeyi günceller
- NativeArray + Job uyumlu yapı ileride Burst Compiler ile 10–50x hızlanma kapısını açık tutuyor
- 8³ chunk boyutu draw call ve güncelleme hızı arasında denge noktası

---

## Hafta 1 Bilinçli Erteleme Listesi

Aşağıdaki konular bilinçli olarak Hafta 1 sonrasına bırakıldı:

- **Smooth normals:** Hafta 1 sonu polish veya Hafta 2
- **Job System + Burst aktivasyonu:** Mimari uyumlu, gerçek ihtiyaca göre eklenecek
- **Material taşma fiziği** (ezilen malzemenin yan tarafa akması): Hafta 3+
- **LOD sistemi:** Gerekirse Hafta 4–5
- **GPU compute shader versiyonu:** Şu anlık gerekmiyor

---

## Hedef Performans Kriterleri

**Karar:**
- **Minimum:** 90 FPS (VR confort eşiği)
- **Hedef:** Stable 90 FPS, occasional 120 FPS
- **Donanım hedefi:** Mid-range PCVR setup (RTX 3060 sınıfı)

---

## Asset Stratejisi

**Karar:** 3B modelleme sıfırdan yapılmayacak, asset mağazalarından tedarik edilecek
**Tarih:** Hafta 1

**Gerekçe:**
- Solo geliştirici + 6 haftalık portfolio kapsamında modelleme süresi yok
- Projenin showcase ettiği asıl şey deformasyon ve VR etkileşim — modelleme ikincil
- Süre mekanik üzerinde harcanmalı

**Birincil kaynaklar:**
- Unity Asset Store
- Fab (Epic'in birleşik mağazası, Quixel Megascans dahil)
- Polyhaven (HDRI/çevre aydınlatması — ücretsiz)

**Reddedilen Alternatif:** Sıfırdan Blender ile modelleme
- Süreyi tüketir, asıl gösterilmek istenen mekaniğe odaklanmayı engellerdi

---

## Ses Yaklaşımı

**Karar:** Ses araçlarına Hafta 4 yaklaşırken karar verilecek (henüz erteli)
**Tarih:** Hafta 1

**Gerekçe:**
- Hafta 4'e kadar deformasyon ve VR çekirdeği oturmalı; ses araç seçimi henüz somut bir karar gerektirmiyor
- Erken karar yerine zamanı geldiğinde proje ihtiyacına göre seçim daha verimli

**Belirlenmesi gereken noktalar:** Ses motoru (Unity native vs. FMOD/Wwise), ses kaynağı (asset, kayıt, kütüphane)

---

## Bütçe ve Güvenlik Politikası

**Karar:** Ücretsiz/açık kaynak tercih edilir, ancak güvenlik öncelikli
**Tarih:** Hafta 1

**Gerekçe:**
- Solo bağımsız proje, ücretli lisanslara bütçe ayrılmıyor
- Ancak denetimsiz/şüpheli açık kaynak çözümlerden gelecek sorunlar (güvenlik açıkları, terk edilmiş projeler, kötü amaçlı kod) vakit ve kaynak kaybettirir
- Bu yüzden seçim kriterleri:
  - Saygın/popüler ve aktif bakımlı projeler
  - Unity resmi paketleri öncelikli
  - Lisansı net, kullanım koşulları açık
  - Topluluk büyüklüğü ve aktif issue takibi olan projeler

**Pratik etkisi:**
- VRIF gibi ücretli ama saygın çözümler ile küçük/güvensiz "ücretsiz" alternatifler arasında VRIF tercih edilebilir
- Auto Hand gibi yaygın kullanılan ve aktif bakımlı asset'ler güvenli bulunuyor

---
## Enchantment Sistemi (Yan Kol)

**Karar:** Yan kol olarak geliştirilecek, ana proje (demir dövme) öncelikli kalacak
**Tarih:** Hafta 1

**Çekirdek formül:** `silah + bileşen = büyülü silah`
**Birincil tetikleyici:** Enchanting Table (ayrı istasyon)
**İleride genişleme:** Aynı mantık dövme sürecine de bağlanabilir (çekiç vuruşunda
bileşen tüketimi). Mimari bunu sıfır maliyetle desteklemeli — tetikleyici
değişir, kural sabit kalır.

### Tasarım Kararları

- **Buff tanımı:** ScriptableObject tabanlı (data-driven). Yeni enchantment
  eklemek tek dosya işi olacak.
- **Bileşen-buff eşleşmesi:** Recipe sistemi. Başlangıçta 1 bileşen = 1 buff,
  ama mimari kombinasyona açık (ileride "ateş tozu + kemik tozu = farklı buff"
  yapılabilir).
- **Bir silahta buff sayısı:** Çoklu desteklenir. Görsel katmanlanma Hafta 4-5
  shader işiyle birlikte ele alınacak.
- **Durum yaşam yeri:** Silahın üzerinde MonoBehaviour component
  (`Enchantable`). Save/load, görsel sync, fizik etkileşimi aynı GameObject
  üzerinden.
- **Görsel katman:** Şimdilik placeholder (MaterialPropertyBlock ile renk/
  emission). Aura/parıltı shader'ı ana proje Hafta 4'te HDRP volumetric/
  emission gündeme gelince birlikte çözülecek.

### Mimari İskelet

| Sınıf | Tip | Sorumluluk |
|---|---|---|
| `Enchantment` | ScriptableObject | Bir buff'ın veri tanımı (ad, etki, görsel preset) |
| `EnchantmentRecipe` | ScriptableObject | "Hangi bileşen → hangi enchantment" eşleşmesi |
| `EnchantmentComponent` | MonoBehaviour | Bileşen item'ı (toz/kristal/taş — `Enchantment` referansı taşır) |
| `Enchantable` | MonoBehaviour | Silahın üzerine takılır, üzerindeki enchantment listesini tutar |
| `EnchantingTable` | MonoBehaviour | Etkileşim noktası — silah + bileşen alır, recipe'a bakar, `Enchantable.Apply()` çağırır |
| `EnchantmentVisuals` | MonoBehaviour | `Enchantable`'ı dinler, görsel preset'i materyale uygular |

**Kritik mimari özellik:** `EnchantingTable` sadece tetikleyici. Çekirdek
mantık `Enchantable.Apply(enchantment)` metodunda. İleride çekiç vuruşundan
da çağrılabilir, hiçbir refactor gerekmez.

### Yan Kol Disiplini

- **Ayrı sahne:** `EnchantmentSandbox.unity` — ana forge sahnesinden bağımsız
  test alanı. Placeholder primitive'ler (küp = silah, top = bileşen, masa
  modeli = table).
- **Ayrı klasör:** `Assets/Scripts/Enchantment/` — demir dövme koduna hiç
  dokunmaz.
- **Entegrasyon zamanlaması:** Demir dövme çekirdeği (Hafta 1-3) tamamen
  oturduktan sonra, muhtemelen Hafta 5 sonrası veya Hafta 6 polish sırasında
  ana sahneye taşınır.

### Açık Sorular (İlerleyince Karar Verilecek)

- Rünler/sembol sistemi (kazıma, çizme) — ileride değerlendirilecek
- Enchantment için asset alımı gerekecek mi (parıltı VFX, partikül paketleri
  vb.) — sandbox olgunlaşınca netleşir