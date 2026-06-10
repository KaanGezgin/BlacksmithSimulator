# Yol Haritası — Blacksmith Simulator

Bu dosya projenin zaman çizelgesini ve milestone'larını tutar.
İlerledikçe güncellenecek, tamamlanan maddeler işaretlenecek.

---

## Mevcut Milestone: Hafta 2 — VR Entegrasyonu

Oturum bölümlemesi: `Assets/Documantations/hafta2-oturum-plani.md`

### Bitiş Kriteri
- VR'da elle tutulan çekiçle bloğa vurunca deformasyon oluyor
- Deformasyon derinliği vuruş sertliğine (impact hızı) göre değişiyor
- Mouse input yerini VR çekiç collision'ı alıyor (mouse masaüstü test için kalır)

### Görevler
- [ ] XR altyapı + VR sahne rig'i (OpenXR/Oculus Touch, HDRP VR, XR Origin) — H2 Oturum 1
- [ ] Çekiç prefab + XRI grab (Velocity Tracking, sap/kafa collider) — H2 Oturum 2
- [ ] Vuruş algılama → deformasyon (impact → brush mapping) — H2 Oturum 3
- [ ] His + polish (haptik, cooldown) — opsiyonel — H2 Oturum 4

---

## Kısa Vadeli Hedefler (1–6 Hafta)

### Hafta 1: Marching Cubes Deformasyon Prototipi
Voxel/chunk altyapısı, mouse ile deformasyon test.

### Hafta 2: VR Entegrasyonu
- XR Interaction Toolkit kurulumu (OpenXR / Oculus Touch)
- XRI native grab — çekiç tutma (Velocity Tracking)
- Çekiç fizikleri (tutma, sallama, çarpma)
- Mouse input yerine VR çekiç collision'ı

### Hafta 3: Isı Sistemi
- Sıcaklığa göre renk geçişi (siyah → kırmızı → turuncu → sarı-beyaz) shader
- Sıcaklık kaybı zaman içinde
- Soğuk demir deformasyona daha az yanıt veriyor
- Ocakta ısıtma mekaniği

### Hafta 4: Ortam, Işıklandırma, Ses
- Demirhane sahnesi (HDRP)
- Volumetric duman, parçacık efektleri (kıvılcımlar, buhar)
- Dinamik ışıklandırma (sıcak metalin emisyonu)
- Temel ses tasarımı (çekiç, ocak, körük, söndürme)

### Hafta 5: UI ve Hedef Şekiller
- Diegetic UI (sipariş tabelası, termometre)
- Hedef şekil sistemi (kılıç, bıçak, nal şablonları)
- Şekil eşleşme/değerlendirme sistemi
- Tamamlanmış işlerin vitrini

### Hafta 6: GitHub Reposu Polish ve Sunum
- README hazırlama (animasyonlu GIF, video, açıklama)
- Mimari diyagramları
- Teknik yazılar (deformasyon sisteminin nasıl çalıştığı)
- Demo build ve release
- Portfolio sayfası entegrasyonu

---

## Orta Vadeli Hedefler (3–12 Ay)

Bu proje portfolio kapsamında olduğu için 6 hafta sonrası genişletme isteğe bağlıdır.
Olası genişlemeler:

- Material taşma fiziği (ezilen malzeme yan tarafa akar)
- Daha fazla hedef şekil ve müşteri tipi
- Su tankında sertleştirme + tavlama mekaniği
- Hammadde çeşitliliği (demir, çelik, bronz)
- Basit ekonomi/progresyon

---

## Uzun Vadeli Vizyon

Şu an yok. Proje portfolio showcase olarak konumlandırılmış durumda.
Tam oyuna dönüşürse ayrı bir planlama yapılacak.

---

## Tamamlanan Milestone'lar

### ✅ Hafta 1 — Marching Cubes Deformasyon Prototipi (Oturum 1–6)

**Bitiş kriteri karşılandı:** 32×16×32 blok mouse ile deforme oluyor, mesh gerçek
zamanlı güncelleniyor, sadece etkilenen chunk'lar yeniden hesaplanıyor (profiler:
ForgeBlock.Update ~0 ms / 0 B GC).

- [x] Mimari tasarım (sınıf sorumlulukları, veri akışı) — Oturum 1
- [x] VoxelData (yoğunluk alanı, deformasyon) — Oturum 1
- [x] Chunk (GameObject + Mesh + Collider) — Oturum 2
- [x] ChunkManager (dirty tracking, mesh regenerasyonu) — Oturum 2
- [x] MarchingCubesTables (EdgeTable + TriTable 256 satır + CornerOffsets/EdgeConnections) — Oturum 3
- [x] MarchingCubesGenerator (algoritmanın kalbi) — Oturum 4
- [x] ForgeBlock (facade + mouse input, iki Input System) — Oturum 5
- [x] Unity'de test ve doğrulama (mouse ile deformasyon çalışıyor) — Oturum 5
- [x] Smooth normals (yoğunluk gradyanı, dikişsiz) — Oturum 6
- [x] Performans profili kontrolü (generator ~64µs/chunk, regen allocation-free) — Oturum 6

---
