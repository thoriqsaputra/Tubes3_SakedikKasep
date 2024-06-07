import sqlite3

def enkripsi_sqlite(input_db_file, output_db_file):
    # Koneksi ke database input
    input_conn = sqlite3.connect(input_db_file)
    input_c = input_conn.cursor()

    # Koneksi ke database output
    output_conn = sqlite3.connect(output_db_file)
    output_c = output_conn.cursor()

    # Membuat tabel di database output jika belum ada
    output_c.execute('''
    CREATE TABLE IF NOT EXISTS biodata (
        NIK VARCHAR(16) PRIMARY KEY NOT NULL, 
        nama VARCHAR(100) DEFAULT NULL, 
        tempat_lahir VARCHAR(50) DEFAULT NULL, 
        tanggal_lahir DATE DEFAULT NULL, 
        jenis_kelamin TEXT, 
        golongan_darah VARCHAR(5) DEFAULT NULL, 
        alamat VARCHAR(255) DEFAULT NULL, 
        agama VARCHAR(50) DEFAULT NULL, 
        status_perkawinan TEXT, 
        pekerjaan VARCHAR(100) DEFAULT NULL, 
        kewarganegaraan VARCHAR(50) DEFAULT NULL
    )
    ''')

    output_c.execute('''
    CREATE TABLE IF NOT EXISTS sidik_jari (
        berkas_citra TEXT,
        nama VARCHAR(100) DEFAULT NULL
    )
    ''')

    # Fungsi untuk mengenkripsi dengan Caesar Cipher
    def caesar_cipher(text, shift):
        result = ""
        for char in text:
            if char.isalpha():
                shift_base = 65 if char.isupper() else 97
                result += chr((ord(char) + shift - shift_base) % 26 + shift_base)
            elif char.isdigit():
                result += chr((ord(char) + shift - 48) % 10 + 48)
            else:
                result += char
        return result

    shift = 4  # Menggunakan pergeseran 4 untuk Caesar Cipher

    # Membaca data dari tabel biodata di database input
    input_c.execute('SELECT * FROM biodata')
    biodata_rows = input_c.fetchall()

    # Mengenkripsi dan memasukkan data ke tabel biodata di database output
    for row in biodata_rows:
        encrypted_row = [
            caesar_cipher(str(row[0]), shift) if row[0] is not None else None,  # NIK
            caesar_cipher(row[1], shift) if row[1] is not None else None,  # nama
            caesar_cipher(row[2], shift) if row[2] is not None else None,  # tempat_lahir
            row[3],  # tanggal_lahir (tidak dienkripsi)
            caesar_cipher(row[4], shift) if row[4] is not None else None,  # jenis_kelamin
            caesar_cipher(row[5], shift) if row[5] is not None else None,  # golongan_darah
            caesar_cipher(row[6], shift) if row[6] is not None else None,  # alamat
            caesar_cipher(row[7], shift) if row[7] is not None else None,  # agama
            caesar_cipher(row[8], shift) if row[8] is not None else None,  # status_perkawinan
            caesar_cipher(row[9], shift) if row[9] is not None else None,  # pekerjaan
            caesar_cipher(row[10], shift) if row[10] is not None else None  # kewarganegaraan
        ]
        output_c.execute('''
        INSERT INTO biodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', encrypted_row)

    # # Membaca data dari tabel sidik_jari di database input
    input_c.execute('SELECT * FROM sidik_jari')
    sidik_jari_rows = input_c.fetchall()

    # Mengenkripsi dan memasukkan data ke tabel sidik_jari di database output
    for row in sidik_jari_rows:
        row = [
            row[0] if row[0] is not None else None,  # berkas_citra
            row[1] if row[1] is not None else None  # nama
        ]
        output_c.execute('''
        INSERT INTO sidik_jari (berkas_citra, nama) VALUES (?, ?)
        ''', row)

    # Menutup koneksi
    input_conn.close()
    output_conn.commit()
    output_conn.close()

    print(f'Data terenkripsi telah disimpan di {output_db_file}')

# Penggunaan script dengan file .db yang dimasukkan dan output
enkripsi_sqlite('biodata.db', 'biodata_encrypted.db')
