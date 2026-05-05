<div align="center">

# &lt;|&gt; M3U8 Downloader

**M3U8 video fayllarini yuklab oluvchi Windows dasturi**

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-blue?style=flat-square)](https://github.com/iZubayr/M3U8Downloader)
[![Runtime](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

---

## 📋 Dastur haqida

**M3U8 Downloader** — m3u8 formatidagi video strimlarni MP4 formatida yuklab olish uchun mo'ljallangan yengil WPF dasturi.

Ko'plab saytlardagi video strimlar aynan `.m3u8` formatida uzatiladi — bu dastur ularni qulay tarzda yuklab olishga yordam beradi.

### Nima qila oladi?

- 🎬 M3U8 video strimlarni **MP4** formatida saqlaydi
- 📋 **Clipboard kuzatish** — m3u8 link nusxalanganda avtomatik aniqlaydi
- 🌐 **Brauzer integratsiyasi** — ochiq brauzer sarlavhasidan video nomini oladi
- ⬇️ **Cheksiz navbat** — bir vaqtda bir nechta yuklanma
- 🇺🇿 / 🇬🇧 O'zbek va Ingliz til qo'llab-quvvatlashi
- 💾 **Papka eslab qoladi** — har safar qayta tanlamaslik uchun
- 📂 Yuklanma papkasini to'g'ridan-to'g'ri ochish
- ⚠️ Yuklanmani bekor qilishda ogohlantirish

---

## ⚠️ Kamchiliklar

| Muammo | Izoh |
|--------|------|
| Progress foizi (%) ko'rsatilmaydi | Ayrim m3u8 strimlar progress ma'lumot bermaydi |
| Brauzerdan avtomatik link olish | CDP porti yopiq bo'lsa ishlamaydi — asosan **qo'lda** nusxalanadi |
| Internetga bog'liqlik | Birinchi ishganda yt-dlp va ffmpeg yuklab olish kerak |

---

## 🚀 O'rnatish

### Talab qilinadigan dasturlar

Dastur birinchi marta ishga tushganda quyidagilarni avtomatik yuklab o'rnatadi:

| Dastur | Hajm | Maqsad |
|--------|------|--------|
| **yt-dlp** | ~12 MB | M3U8 strimni yuklab olish mexanizmi |
| **ffmpeg** | ~115 MB | Video va audio fayllarni birlashtirish |

> 💡 Agar `yt-dlp` va `ffmpeg` kompyuterda allaqachon o'rnatilgan bo'lsa, setup ekranida **"O'tkazib yuborish"** tugmasini bosing.

### .NET 8

| Maqsad | Kerak bo'ladigan narsa | Havola |
|--------|----------------------|--------|
| **Dasturni ishlatish** | .NET 8 Desktop Runtime (~55 MB) | [Yuklab olish](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.16-windows-x64-installer) |
| **Kodni build qilish** | .NET 8 SDK (~200 MB) | [Yuklab olish](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) |

> 💡 Faqat dasturni ishlatmoqchi bo'lsangiz — **Runtime** yetarli. SDK faqat kodni o'zingiz build qilmoqchi bo'lsangiz kerak.

---

## 📥 Yuklab olish

**[➡️ Releases bo'limidan .exe yuklab oling](https://github.com/iZubayr/M3U8Downloader/releases)**

---

## 🔨 Build qilish

```bash
# 1. Reponi clone qiling
git clone https://github.com/iZubayr/M3U8Downloader.git

# 2. Papkaga kiring
cd M3U8Downloader

# 3. Build qiling
build.bat
```

**Talab:** [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

---

## 📖 Ishlatish

1. `M3U8Downloader.exe` ni ishga tushiring — **Administrator** huquqi so'raladi
2. Birinchi marta: `yt-dlp` va `ffmpeg` avtomatik yuklanadi
3. M3U8 linkni **clipboard ga nusxalang** — dastur avtomatik aniqlaydi
4. Yoki linkni **qo'lda** kiriting
5. Video nomini kiriting (yoki brauzer sarlavhasidan avtomatik olinadi)
6. Saqlash papkasini tanlang
7. **"Navbatga qo'shish"** tugmasini bosing

---

## 🔧 Brauzerdan link olish (CDP)

`🌐 Brauzer` tugmasi Chrome/Edge ni **remote debugging** orqali tekshiradi.

Ishlashi uchun brauzer quyidagicha ochilishi kerak:

```
chrome.exe --remote-debugging-port=9222
```

Aks holda linkni **qo'lda nusxalash** tavsiya etiladi.

---

## 📁 Fayl joylashuvi

```
M3U8Downloader.exe
│
├── %LocalAppData%\M3U8Tools\
│     ├── yt-dlp.exe        ← avtomatik yuklanadi
│     └── ffmpeg.exe        ← avtomatik yuklanadi
│
└── %AppData%\M3U8Downloader\
      └── config.json       ← sozlamalar
```

---


<div align="center">

Made with ❤️ by [iZubayr](https://github.com/iZubayr)

</div>
