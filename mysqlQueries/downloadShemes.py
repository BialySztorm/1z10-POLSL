import json
import pymysql
import os
import re


def connect_to_db():
    # print('Getting credentials')
    with open('../secrets.json') as f:
        credentials = json.load(f)
    host, port, user, passwd, db = credentials['Database:server'], credentials['Database:port'], credentials['Database:username'], credentials['Database:password'], credentials['Database:database']
    # print('Credentials', credentials)
    connection = pymysql.connect(host=host,
                                 port=int(port),
                                 user=user,
                                 password=passwd,
                                 db=db,
                                 charset='utf8mb4',
                                 cursorclass=pymysql.cursors.DictCursor)
    return connection


def get_table_relations(connection, table_name):
    with connection.cursor() as cursor:
        cursor.execute(f"SHOW CREATE TABLE `{table_name}`")
        create_table_statement = cursor.fetchone()['Create Table']
        # print(create_table_statement)

        # Wyszukaj linie definiujące klucze obce
        foreign_key_lines = [line.strip() for line in create_table_statement.split('\n') if 'FOREIGN KEY' in line]
        # print(foreign_key_lines)

        # Wyciągnij nazwy tabel i kolumn, do których odnoszą się klucze obce
        relations = []
        for line in foreign_key_lines:
            match = re.search(r'FOREIGN KEY \("(.+?)"\) REFERENCES "(.+?)" \("(.+?)"\)', line)
            if match:
                relations.append({
                    'from_column': match.group(1),
                    'to_table': match.group(2),
                    'to_column': match.group(3)
                })
        print(relations)

    return relations


def get_database_schema_and_relations(connection):
    try:
        with connection.cursor() as cursor:
            # Pobierz listę tabel
            cursor.execute("SHOW TABLES")
            tables = [table['Tables_in_defaultdb'] for table in cursor.fetchall()]

            # Pobierz schemat dla każdej tabeli
            schema = {}
            for table in tables:
                cursor.execute(f"SHOW CREATE TABLE `{table}`")
                schema[table] = cursor.fetchone()['Create Table']

        relations = {}
        for table in tables:
            relations[table] = get_table_relations(connection, table)

        return schema, relations
    finally:
        connection.close()


def create_directories_if_not_exist():
    if not os.path.exists("schemes"):
        os.makedirs("schemes")
    if not os.path.exists("relations"):
        os.makedirs("relations")


def save_schemes(schemes):
    create_directories_if_not_exist()
    for scheme in schemes:
        with open(f"schemes/{scheme}.sql", "w") as file:
            file.write(schemes[scheme])


def save_relations(relations, table_name):
    create_directories_if_not_exist()
    with open(f"relations/{table_name}.json", "w") as file:
        json.dump(relations, file)


connection = connect_to_db()

schema, relations = get_database_schema_and_relations(connection)
save_schemes(schema)
for table in relations:
    save_relations(relations[table], table)
