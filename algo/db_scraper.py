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
    NIK TEXT, 
    nama TEXT, 
    tempat_lahir TEXT, 
    tanggal_lahir TEXT, 
    jenis_kelamin TEXT, 
    golongan_darah TEXT, 
    alamat TEXT, 
    agama TEXT, 
    status_perkawinan TEXT, 
    pekerjaan TEXT, 
    kewarganegaraan TEXT
)
''')

# Generate records
for _ in range(num_records):
    c.execute('''
    INSERT INTO biodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    ''', (
        str(fake.unique.random_number(digits=16, fix_len=True)),
        alay_converter_advanced(fake.name()),
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

conn.commit()

conn.close()

print(f'{num_records} records generated and saved to biodata.db')
