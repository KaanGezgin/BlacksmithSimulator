# Yol Haritası — Blacksmith Simulator

Bu dosya projenin zaman çizelgesini ve milestone'larını tutar.
İlerledikçe güncellenecek, tamamlanan maddeler işaretlenecek.

---

## Mevcut Milestone: Hafta 1 — Marching Cubes Deformasyon Prototipi

### Bitiş Kriteri
- 32×16×32 voxel'lik bir blok mouse tıklamasıyla deforme edilebiliyor
- Mesh gerçek zamanlı güncelleniyor (60+ FPS, henüz VR yok)
- Chunk sistemi çalışıyor, sadece etkilenen chunk'lar yeniden hesaplanıyor

### Görevler
- [x] Mimari tasarım (sınıf sorumlulukları, veri akışı) — Oturum 1
- [x] VoxelData (yoğunluk alanı, deformasyon) — Oturum 1
- [x] Chunk (GameObject + Mesh + Collider) — Oturum 2
- [x] ChunkManager (dirty tracking, mesh regenerasyonu) — Oturum 2
- [ ] MarchingCubesGenerator (algoritmanın kalbi)
- [ ] MarchingCubesTables (lookup tabloları — TriTable tamamlanması gerekiyor)
- [ ] ForgeBlock (facade + mouse input)
- [ ] Unity'de test ve doğrulama
- [ ] TriTable tam 256 satırın eklenmesi
- [ ] Smooth normals (polish)
- [ ] Performans profili kontrolü

---

## Kısa Vadeli Hedefler (1–6 Hafta)

### Hafta 1: Marching Cubes Deformasyon Prototipi
Voxel/chunk altyapısı, mouse ile deformasyon test.

### Hafta 2: VR Entegrasyonu
- XR Interaction Toolkit kurulumu
- Auto Hand entegrasyonu
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

*(henüz yok — Hafta 1 devam ediyor)*

---
