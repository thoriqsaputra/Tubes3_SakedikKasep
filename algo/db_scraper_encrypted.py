import sqlite3
import random
from faker import Faker
from alay_generator import alay_converter_advanced
from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives import serialization

# Generate RSA keys
private_key = rsa.generate_private_key(
    public_exponent=65537,
    key_size=2048
)
public_key = private_key.public_key()

# serialize the private key
pem_private_key = private_key.private_bytes(
    encoding=serialization.Encoding.PEM,
    format=serialization.PrivateFormat.PKCS8,
    encryption_algorithm=serialization.NoEncryption()
)

# serialize the public key
pem_public_key = public_key.public_bytes(
    encoding=serialization.Encoding.PEM,
    format=serialization.PublicFormat.SubjectPublicKeyInfo
)

def encrypt_rsa(public_key, message):
    ciphertext = public_key.encrypt(
        message.encode('utf-8'),
        padding.OAEP(
            mgf=padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )
    return ciphertext

def decrypt_rsa(private_key, ciphertext):
    plaintext = private_key.decrypt(
        ciphertext,
        padding.OAEP(
            mgf=padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )
    return plaintext.decode('utf-8')

# save private key and public key
with open("Key/private_key.pem", "wb") as f:
    f.write(pem_private_key)

with open("Key/public_key.pem", "wb") as f:
    f.write(pem_public_key)

# load private key
with open("Key/private_key.pem", "rb") as f:
    private_key = serialization.load_pem_private_key(
        f.read(),
        password=None,
    )

# load public key
with open("Key/public_key.pem", "rb") as f:
    public_key = serialization.load_pem_public_key(
        f.read()
    )

fake = Faker('id_ID')

conn = sqlite3.connect('biodata.db')
c = conn.cursor()

c.execute('''
CREATE TABLE IF NOT EXISTS biodata (
    NIK VARCHAR(16) PRIMARY KEY NOT NULL, 
    nama BLOB DEFAULT NULL, 
    tempat_lahir BLOB DEFAULT NULL, 
    tanggal_lahir BLOB DEFAULT NULL, 
    jenis_kelamin BLOB, 
    golongan_darah BLOB DEFAULT NULL, 
    alamat BLOB DEFAULT NULL, 
    agama BLOB DEFAULT NULL, 
    status_perkawinan BLOB, 
    pekerjaan BLOB DEFAULT NULL, 
    kewarganegaraan BLOB DEFAULT NULL
)
''')

c.execute('''
CREATE TABLE IF NOT EXISTS sidik_jari (
    berkas_citra TEXT,
    nama BLOB DEFAULT NULL
)
''')

id = 1
for i in range(2000):
    data_nama = fake.name()
    encrypted_nama = encrypt_rsa(public_key, alay_converter_advanced(data_nama, use_number_symbol=True, use_case_mix=False, use_vowel_removal=False))
    c.execute('''
    INSERT INTO biodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    ''', (
        str(fake.unique.random_number(digits=16, fix_len=True)),
        encrypted_nama,
        encrypt_rsa(public_key, fake.city()),
        encrypt_rsa(public_key, fake.date_of_birth(minimum_age=18, maximum_age=90).strftime('%Y-%m-%d')),
        encrypt_rsa(public_key, random.choice(['Laki-Laki', 'Perempuan'])),
        encrypt_rsa(public_key, random.choice(['A', 'B', 'AB', 'O'])),
        encrypt_rsa(public_key, fake.address().replace('\n', ', ')),
        encrypt_rsa(public_key, random.choice(['Islam', 'Kristen', 'Katolik', 'Hindu', 'Buddha', 'Konghucu'])),
        encrypt_rsa(public_key, random.choice(['Belum Menikah', 'Menikah', 'Cerai'])),
        encrypt_rsa(public_key, fake.job()),
        encrypt_rsa(public_key, 'Indonesia')
    ))
    c.execute('''
    INSERT INTO sidik_jari (berkas_citra, nama) VALUES (?, ?)
    ''', (
        str(id),
        encrypt_rsa(public_key, data_nama)
    ))
    id += 1

conn.commit()

# Decrypting the data from the database
# c.execute('SELECT NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan FROM biodata')
# rows = c.fetchall()

# for row in rows:
#     decrypted_row = [
#         row[0],  # NIK is not encrypted
#         decrypt_rsa(private_key, row[1]),
#         decrypt_rsa(private_key, row[2]),
#         decrypt_rsa(private_key, row[3]),
#         decrypt_rsa(private_key, row[4]),
#         decrypt_rsa(private_key, row[5]),
#         decrypt_rsa(private_key, row[6]),
#         decrypt_rsa(private_key, row[7]),
#         decrypt_rsa(private_key, row[8]),
#         decrypt_rsa(private_key, row[9]),
#         decrypt_rsa(private_key, row[10]),
#     ]
#     print(decrypted_row)

conn.close()
