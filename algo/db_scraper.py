import sqlite3
import random
from faker import Faker
from alay_generator import alay_converter_advanced, revert_alay_name

fake = Faker('id_ID')

num_records = 6000

conn = sqlite3.connect('biodata.db')
c = conn.cursor()

# Membuat tabel jika belum ada
c.execute('''
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

c.execute('''
CREATE TABLE IF NOT EXISTS sidik_jari (
    berkas_citra TEXT,
    nama VARCHAR(100) DEFAULT NULL
)
''')

# Generate records
for i in range(num_records):
    data_nama = fake.name()
    c.execute('''
    INSERT INTO biodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    ''', (
        str(fake.unique.random_number(digits=16, fix_len=True)),
        alay_converter_advanced(data_nama, use_number_symbol=random.choice([True, False]), use_case_mix=random.choice([True, False]), use_vowel_removal=random.choice([True, False])),
        fake.city(),
        fake.date_of_birth(minimum_age=18, maximum_age=90).strftime('%Y-%m-%d'),
        random.choice(['Laki-Laki', 'Perempuan']),
        random.choice(['A', 'B', 'AB', 'O']),
        fake.address().replace('\n', ', '),
        random.choice(['Islam', 'Kristen', 'Katolik', 'Hindu', 'Buddha', 'Konghucu']),
        random.choice(['Belum Menikah', 'Menikah', 'Cerai']),
        fake.job(),
        'Indonesia'
    ))
    c.execute('''
    INSERT INTO sidik_jari (berkas_citra, nama) VALUES (?, ?)
    ''', (
        str(i),
        data_nama
    ))

conn.commit()

conn.close()

print(f'{num_records} records generated and saved to biodata.db')
