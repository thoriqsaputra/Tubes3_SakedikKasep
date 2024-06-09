# Tubes 3 Strategi Algoritma
### Pemanfaatan Pattern Matching dalam Membangun Sistem Deteksi Individu Berbasis Biometrik Melalui Citra Sidik Jari


> Tugas besar ini adalah implementasi dari algoritma Boyer-Moore dan Knuth-Morris-Pratt untuk melakukan identifikasi biometrik berbasis sidik jari. Identifikasi biometrik ini bertujuan untuk mencocokkan sidik jari dengan data yang ada dalam database, menggunakan kedua algoritma pencarian string yang telah diajarkan pada materi dan salindia kuliah. Dalam tugas kecil ini, kami mengimplementasikan kedua algoritma tersebut dalam bahasa pemrograman yang relevan dan melakukan pengujian untuk membandingkan kinerja dan efektivitas masing-masing algoritma. Algoritma Knuth-Morris-Pratt (KMP) bekerja dengan cara membangun tabel partial match yang digunakan untuk menghindari pemeriksaan ulang karakter yang telah dicocokkan, sehingga meningkatkan efisiensi pencarian string. Algoritma Boyer-Moore (BM) menggunakan dua heuristik utama, yaitu bad character rule dan good suffix rule, untuk melompati lebih banyak karakter dalam teks selama proses pencarian, menjadikannya sangat efisien untuk teks yang panjang. Ekspresi reguler (regex) adalah alat yang kuat untuk pencocokan pola yang memungkinkan deskripsi dan pencocokan pola yang kompleks dengan menggunakan sintaks khusus, yang juga diimplementasikan dalam tubes ini untuk melengkapi proses identifikasi.

## Table of Contents
* [Technologies Used](#technologies-used)
* [Features](#features)
* [Setup](#setup)
* [Usage](#usage)
* [Project Status](#project-status)
* [Contact](#contact)

## Technologies Used
- C#
- Windows Presentation Foundation

## Features
- Algoritma Knuth–Morris–Pratt
- Algoritma Boyer–Moore

## Setup
Untuk kepastian program berjalan sesuai dengan keinginan anda dapat melakukan setup sebagai berikut:

- Clone repo ini pada directory yang diinginkan.
```bash
git clone https://github.com/thoriqsaputra/Tubes3_SakedikKasep
```

## Usage
#### Menjalankan Program:
- Persiapkan dataset sidik jari: Tempatkan dataset sidik jari di folder test dan database bio di folder src. Jika tidak ada dataset sendiri, gunakan dataset yang tersedia di repository.

- Ubah directory ke src: Buka terminal, pastikan berada di directory awal project, dan jalankan command berikut:

```bash
cd src
```

- Jalankan GUI: Di folder src, eksekusi perintah berikut:

```bash
dotnet run
```
- Gunakan GUI: GUI akan muncul dan siap digunakan.

#### Menggunakan Program:
- Unggah gambar sidik jari Anda terlebih dahulu dengan mengklik gambar sidik jari bagian kiri dan pastikan gambar yang diunggah jelas serta sesuai dengan format yang diizinkan.
- Setelah gambar sidik jari berhasil diunggah, pilih salah satu dari dua algoritma yang tersedia untuk mempengaruhi proses pencarian dan hasil akhir.
- Jalankan program dengan menekan tombol "Start" setelah memilih algoritma untuk memulai pencarian sidik jari.
- Tunggu beberapa saat hingga program selesai memproses gambar sidik jari dan hasil pencarian akan ditampilkan di layar.


## Project Status
Project is: _complete_

## Contact
Created by:
- Rizqika Mulia Pratama (13522126)
- Ahmad Thoriq Saputra (13522141)
- Muhammad Fatihul Irhab (13522143)

