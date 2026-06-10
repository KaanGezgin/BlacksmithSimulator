# Hafta 1 — Oturum Planı (Token Bütçesine Göre Parçalı)

Bu dosya Hafta 1'i (Marching Cubes Deformasyon Prototipi) **token kullanımına
göre parçalanmış oturumlara** böler. Amaç: 5 saatlik kullanım penceresi veya
genel kullanım limitine takılırsan, işin bir görevin *ortasında* değil, iki
oturum *arasındaki temiz sınırda* kesilmesi.

## Kullanım Kuralları

1. **Bir oturumda sadece o oturumun kapsamını yap.** Erken bitirirsen sonrakine
   geçme — bilinçli dur, limit penceresini koru.
2. Her oturumun sonunda `yol-haritasi.md` içindeki ilgili görevleri işaretle.
   Bu senin "resume noktan" olur; yeni sohbette buradan devam edilir.
3. Yeni bir oturuma başlarken Claude'a sadece şunu söyle:
   *"Hafta 1 Oturum N'e başlıyoruz, plan dosyasına bak."*
4. **Token tasarrufu:** Sabit/bilinen veri (özellikle TriTable) sıfırdan
   üretmek yerine güvenilir referanstan (Paul Bourke / Sebastian Lague MIT repo)
   alınır. Referans linkleri `teknik-stack.md` içinde.

## Ağırlık Lejantı

- 🟢 **Hafif** — çoğunlukla tartışma + küçük kod, düşük çıktı tokenı
- 🟡 **Orta** — bir-iki sınıf, makul kod çıktısı
- 🔴 **Ağır** — büyük kod çıktısı veya yoğun iterasyon; **tek başına izole edildi**

---

## Oturum 1 — Mimari + Veri Temeli  🟡

**Kapsam**
- [ ] Mimari tasarım (sınıf sorumlulukları, veri akışı, namespace/klasör)
- [ ] `VoxelData` (yoğunluk alanı, deformasyon API'si)

**Neden birlikte:** Mimari tartışması çoğunlukla metin (düşük token), `VoxelData`
Unity'ye hiç bağımlı olmayan saf veri sınıfı. İkisi birlikte temeli kurar.

**Temiz sınır:** Mimari onaylandı + `VoxelData` derleniyor. Henüz Unity sahnesine
hiçbir şey eklenmedi, mesh yok. Burada kesilmek güvenli.

**Resume sinyali:** `yol-haritasi.md` → ilk iki görev işaretli.

---

## Oturum 2 — Chunk + ChunkManager  🟡

**Kapsam**
- [ ] `Chunk` (GameObject + Mesh + MeshCollider sarmalayıcı)
- [ ] `ChunkManager` (dirty tracking, mesh regenerasyon orkestrasyonu)

**Neden birlikte:** İkisi sıkı bağlı; `ChunkManager` `Chunk`'ları yönetir.
Birlikte yazmak ileri-geri tartışmayı azaltır (token tasarrufu).

**Temiz sınır:** Chunk yaşam döngüsü ve dirty-tracking iskeleti hazır. Mesh
üretimi henüz boş/stub (Marching Cubes Oturum 3-4'te gelecek). Burada kesilmek
güvenli — generator'a hiç dokunulmadı.

**Resume sinyali:** `Chunk` ve `ChunkManager` görevleri işaretli.

---

## Oturum 3 — Marching Cubes Tabloları  🔴 (EN AĞIR — İZOLE)

**Kapsam**
- [ ] `MarchingCubesTables` — EdgeTable (256) + **TriTable tam 256 satır**

**Neden tek başına:** TriTable, tüm Hafta 1'in **en büyük tek token çıktısı**
(256 satır × 16 değere kadar). Eğer bir yerde limite takılacaksan en olası yer
burası. Tek başına izole edilince, olası bir kesinti başka hiçbir işi boşa
harcamaz — sadece bu tabloyu kaldığın satırdan sürdürürsün.

**Token tasarrufu (önemli):** Bu tablo evrensel bir sabittir. Sıfırdan
"üretmek" yerine referanstan (Paul Bourke / Sebastian Lague MIT) alınıp C#
formatına uyarlanır. Claude'a bunu hatırlat.

**Temiz sınır:** Her iki tablo da tam ve derleniyor. Kesinti olursa: tablonun
hangi satıra kadar dolduğu kodda görünür → oradan devam.

**Resume sinyali:** `MarchingCubesTables` + "TriTable tam 256 satır" işaretli.

---

## Oturum 4 — MarchingCubesGenerator  🔴

**Kapsam**
- [ ] `MarchingCubesGenerator` (algoritmanın kalbi — interpolasyon, mesh üretimi)

**Neden tek başına:** Algoritma yoğun kod + dikkatli mantık. Oturum 3'teki
tabloları tüketir, o yüzden ondan sonra. Tek başına tutuldu çünkü iterasyon
(köşe indeksleme, vertex interpolasyonu) öngörülemez token harcayabilir.

**Temiz sınır:** Generator bir chunk için doğru mesh üretiyor (henüz Unity
sahnesinde mouse ile test edilmedi). Saf algoritma seviyesinde tamam.

**Resume sinyali:** `MarchingCubesGenerator` işaretli.

---

## Oturum 5 — ForgeBlock + Unity Entegrasyon & Test  🟡

**Kapsam**
- [ ] `ForgeBlock` (facade + mouse input)
- [ ] Unity'de test ve doğrulama (sahne kurulumu, raycast deformasyon)

**Neden birlikte:** `ForgeBlock` tüm parçaları birleştiren ince katman; gerçek
testi ancak o varken yapılır. Test iteratif ama düzeltmeler küçük olur.

**Dikkat:** Test/doğrulama iterasyonu öngörülemez. Eğer bu oturum uzarsa,
**ForgeBlock yazıldıktan sonra** doğal bir mola noktası var — test bir sonraki
pencereye bırakılabilir.

**Temiz sınır:** Bitiş kriteri karşılanıyor — 32×16×32 blok mouse ile deforme
oluyor, mesh gerçek zamanlı güncelleniyor, sadece etkilenen chunk yeniden
hesaplanıyor.

**Resume sinyali:** `ForgeBlock` + "Unity'de test ve doğrulama" işaretli.
**Hafta 1'in çekirdeği burada biter.**

---

## Oturum 6 — Polish (Opsiyonel, Ayrı)  🟢

**Kapsam**
- [ ] Smooth normals
- [ ] Performans profili kontrolü

**Neden ayrı:** İkisi de "nice-to-have" polish. `kararlar.md` bunları zaten
bilinçli olarak Hafta 1 sonu / Hafta 2'ye ertelenebilir işaretlemiş. Çekirdek
(Oturum 5) bittiğinde Hafta 1'in bitiş kriteri zaten karşılanmış olur; bu oturum
kesilse bile prototip çalışır durumda kalır.

**Temiz sınır:** Hafta 1 tamamen kapanır → `yol-haritasi.md` "Tamamlanan
Milestone'lar" bölümüne taşınır.

---

## Özet Tablo

| Oturum | Kapsam | Ağırlık | Kesinti güvenli mi? |
|---|---|---|---|
| 1 | Mimari + VoxelData | 🟡 Orta | ✅ |
| 2 | Chunk + ChunkManager | 🟡 Orta | ✅ |
| 3 | MC Tabloları (TriTable) | 🔴 Ağır | ✅ (izole) |
| 4 | MarchingCubesGenerator | 🔴 Ağır | ✅ |
| 5 | ForgeBlock + Test | 🟡 Orta | ✅ |
| 6 | Polish (opsiyonel) | 🟢 Hafif | ✅ |

**Önerilen tempo:** Bir kullanım penceresinde 1 oturum (ağır olanlarda 3 ve 4'ü
asla birleştirme). Pencere genişse 1+2 veya 5+6 birleştirilebilir, ama 3 ve 4
her zaman yalnız kalsın.
