# Hafta 2 — Oturum Planı (Token Bütçesine Göre Parçalı)

Bu dosya Hafta 2'yi (VR Entegrasyonu) **token kullanımına göre parçalanmış
oturumlara** böler. Amaç Hafta 1 ile aynı: 5 saatlik kullanım penceresine veya
genel limite takılırsan, işin bir görevin *ortasında* değil, iki oturum
*arasındaki temiz sınırda* kesilmesi.

> Hafta 2'nin büyük kısmı **senin Unity editöründe yapacağın asset/config işidir**
> (paket kurulumu, XR ayarları, prefab sürükleme). Claude'un çıktısı çoğunlukla
> *rehberlik + birkaç glue script*'tir. O yüzden config-ağırlıklı oturumlarda
> token düşük; ağır olan tek kısım vuruş→deformasyon impact tuning.

## Karar bağlamı (2026-06-10)

VR grab yaklaşımı **Auto Hand'den XRI native grab'e** revize edildi (`kararlar.md`,
maliyet ~$90). Artık 3rd-party fiziksel-el asset'i yok: çekiç `XRGrabInteractable`
+ **Velocity Tracking** ile tutulur, fiziksel hızla bloğa çarpar, `relativeVelocity`
okunup `ForgeBlock.DeformAt()`'e haritalanır. Bu, eski "Auto Hand entegrasyonu"
oturumunu ortadan kaldırdı → plan 5 yerine **4 oturum**.

## Ön Koşullar ve Riskler (Oturuma başlamadan oku)

1. **VR headset:** Meta Quest, PC'ye **Link / Air Link** ile bağlı. OpenXR'de
   **Oculus Touch** controller interaction profile'ı seçilir (Oturum 1).
2. **HDRP + VR dikkat noktası:** HDRP VR'ı destekler ama XR render ayarı
   (single-pass instanced) ve uyumlu HDRP sürümü gerekir. Oturum 1'de ayarlanır;
   bir bölüm burada en olası sürtünme noktasıdır.
3. **Ek ücretli asset yok:** XRI ücretsiz ve resmi; çekiç modeli placeholder
   (Unity primitive) veya ücretsiz asset olabilir.
4. **Mouse input korunur:** `ForgeBlock`'taki mouse yolu silinmez; Oturum 3'te
   VR collision yanına koşullu olarak bırakılır (masaüstünde hızlı test için).

## Kullanım Kuralları

1. **Bir oturumda sadece o oturumun kapsamını yap.** Erken bitirirsen sonrakine
   geçme — bilinçli dur, limit penceresini koru.
2. Her oturumun sonunda `yol-haritasi.md` içindeki ilgili görevleri `[x]` işaretle
   ("— H2 Oturum N" notuyla). Bu senin "resume noktan" olur.
3. Yeni bir oturuma başlarken Claude'a sadece şunu söyle:
   *"Hafta 2 Oturum N'e başlıyoruz, plan dosyasına bak."*
4. **Token tasarrufu:** Hazır asset (XRI prefab'leri, çekiç modeli, controller
   profilleri) sıfırdan yazılmaz; mevcut çözüm kullanılır.

## Ağırlık Lejantı

- 🟢 **Hafif** — çoğunlukla rehberlik + küçük kod/config, düşük çıktı tokenı
- 🟡 **Orta** — config zinciri veya bir-iki glue script, makul çıktı
- 🔴 **Ağır** — yoğun iterasyon/tuning; **izole edildi**

---

## Oturum 1 — XR Altyapı + VR Sahne Rig'i  🟡

**Kapsam**
- [ ] XR Plugin Management + OpenXR Plugin kurulumu, **Oculus Touch** interaction profile
- [ ] HDRP VR render ayarları (XR Settings, single-pass instanced)
- [ ] XR Interaction Toolkit paketi + XR Origin (camera rig) sahneye
- [ ] Quest'te (Link) kafa + el (controller) takibi görünür

**Neden birlikte:** Hepsi "VR'da sahneyi görene kadar"ki tek bir config zinciri;
çoğu editör işi (düşük token), glue kod yok.

**Temiz sınır:** Headset taktığında forge sahnesini VR'da görüyorsun, başını ve
controller'larını hareket ettirebiliyorsun. Henüz etkileşim yok — hiç glue kod
yazılmadığı için burada kesilmek tamamen güvenli.

**Resume sinyali:** `yol-haritasi.md` → "XR altyapı + VR sahne rig'i" işaretli.

---

## Oturum 2 — Çekiç Prefab + XRI Grab  🟡

**Kapsam**
- [ ] Çekiç modeli (placeholder primitive veya ücretsiz asset), Rigidbody + sap collider
- [ ] `XRGrabInteractable` + **Movement Type = Velocity Tracking** → çekiç fizikle
      controller'ı takip ediyor, elle tutulabiliyor
- [ ] Çekiç **kafasına** ayrı convex collider (vuruş algılama için — henüz mantık yok)
- [ ] El görseli (controller modeli veya XRI sample el modeli)

**Neden birlikte:** XRI native grab kurulumu hafif; çekiç fiziği, grab ayarı ve
collider yapısı (sap vs. kafa) tek bir bütün olarak kurulur. (Auto Hand'in ayrı
ağır oturumu artık yok.)

**Temiz sınır:** Çekici elinle tutup savurabiliyorsun, fizik stabil, kafa collider'ı
hazır. Vuruş henüz hiçbir şey yapmıyor — bağlı değil. Güvenli sınır.

**Resume sinyali:** "Çekiç prefab + XRI grab" işaretli.

---

## Oturum 3 — Vuruş Algılama → Deformasyon  🔴 (iterasyon — İZOLE)

**Kapsam**
- [ ] Çekiç kafası `OnCollisionEnter` → `ForgeBlock.DeformAt(temasNoktası)` çağrısı
- [ ] Impact hızı (`relativeVelocity`) → brush strength/radius haritalama (sert vuruş = derin çukur)
- [ ] Min impact eşiği (hafif dokunuş deforme etmesin) + mouse input'u koşullu

**Neden tek başına:** collision→deformasyon köprüsü ve impact tuning iteratif,
öngörülemez (Hafta 1'deki generator gibi). `DeformAt(worldPoint)` zaten hazır
giriş noktası — bu oturum yalnızca VR vuruşunu oraya bağlar ve hissi ayarlar.
Çekiç kafası convex collider, blok chunk'larının non-convex MeshCollider'ına çarpar
(Unity destekler; `OnCollisionEnter` tetiklenir).

**Temiz sınır:** VR'da çekiçle bloğa vurunca, vuruş sertliğine göre deforme oluyor.
**Hafta 2'nin çekirdek bitiş kriteri burada karşılanır** (mouse yerine VR çekiç
collision'ı).

**Resume sinyali:** "Vuruş algılama → deformasyon" işaretli.

---

## Oturum 4 — His + Polish (Opsiyonel, Ayrı)  🟢

**Kapsam**
- [ ] Haptik geri bildirim (vuruşta controller titreşimi, impact'e oranlı)
- [ ] Vuruş cooldown / debounce, çoklu temas filtreleme
- [ ] Çekici masaya/örse koyup alma, ergonomi

**Neden ayrı:** His ayarı nice-to-have. Çekirdek (Oturum 3) bitince Hafta 2'nin
bitiş kriteri zaten karşılanmış olur; bu oturum kesilse bile VR dövme çalışır
durumda kalır.

**Temiz sınır:** Dövme hissi tatmin edici, demo'ya hazır → Hafta 2 kapanır.

---

## Özet Tablo

| Oturum | Kapsam | Ağırlık | Kesinti güvenli mi? |
|---|---|---|---|
| 1 | XR altyapı + VR sahne rig'i | 🟡 Orta | ✅ |
| 2 | Çekiç prefab + XRI grab | 🟡 Orta | ✅ |
| 3 | Vuruş algılama → deformasyon | 🔴 Ağır | ✅ (izole) |
| 4 | His + polish (opsiyonel) | 🟢 Hafif | ✅ |

**Önerilen tempo:** Bir pencerede 1 oturum. Pencere genişse 1+2 birleştirilebilir,
ama **3 (impact tuning) her zaman yalnız kalsın** — iterasyon ağırlıklı, Hafta
1'deki generator oturumu gibi.
