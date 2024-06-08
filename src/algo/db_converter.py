import sqlite3
import os

def convert_sql_to_sqlite(sql_file, sqlite_db):
    if not os.path.exists(sql_file):
        print(f"File {sql_file} tidak ditemukan.")
        return
    
    with open(sql_file, 'r') as file:
        sql_script = file.read()
    
    conn = sqlite3.connect(sqlite_db)
    cursor = conn.cursor()
    
    cursor.executescript(sql_script)
    
    conn.commit()
    conn.close()
    print(f"File {sql_file} berhasil dikonversi ke {sqlite_db}")

def convert_sqlite_to_sql(sqlite_db, sql_file):
    if not os.path.exists(sqlite_db):
        print(f"Database {sqlite_db} tidak ditemukan.")
        return

    conn = sqlite3.connect(sqlite_db)
    cursor = conn.cursor()
    
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';")
    tables = cursor.fetchall()

    
    with open(sql_file, 'w') as file:
        for table in tables:
            table_name = table[0]
            
            cursor.execute(f"SELECT sql FROM sqlite_master WHERE type='table' AND name='{table_name}';")
            create_table_script = cursor.fetchone()[0]
            file.write(f"{create_table_script};\n\n")
            
            cursor.execute(f"SELECT * FROM {table_name}")
            rows = cursor.fetchall()
            
            for row in rows:
                values = ', '.join([f"'{str(value)}'" if value is not None else 'NULL' for value in row])
                insert_statement = f"INSERT INTO {table_name} VALUES ({values});"
                file.write(f"{insert_statement}\n")
            
            file.write("\n")
    
    conn.close()
    
    if os.path.exists(sql_file):
        print(f"Database {sqlite_db} berhasil dikonversi ke {sql_file}")
    else:
        print(f"File {sql_file} tidak ditemukan setelah konversi")

sqlite_db_to_convert = 'output_sqlite.db'
sql_file_output = 'output_sqlfile.sql'
convert_sql_to_sqlite(sql_file_output, sqlite_db_to_convert)
