import os
import pymysql
import json
import datetime


def connect_to_db():
    print('Getting credentials')
    with open('../secrets.json') as f:
        credentials = json.load(f)
    host, port, user, passwd, db = credentials['Database:server'], credentials['Database:port'], credentials['Database:username'], credentials['Database:password'], credentials['Database:database']
    connection = pymysql.connect(host=host,
                                 port=int(port),
                                 user=user,
                                 password=passwd,
                                 db=db,
                                 charset='utf8mb4',
                                 cursorclass=pymysql.cursors.DictCursor)
    return connection


def date_handler(obj):
    if isinstance(obj, (datetime.datetime, datetime.date)):
        return obj.isoformat()
    raise TypeError(f"Type {type(obj)} not serializable")


def get_data():
    print('Getting data')
    connection = connect_to_db()
    cursor = connection.cursor()

    # Get table names
    cursor.execute("SHOW TABLES")
    tables = cursor.fetchall()

    # For each table, execute SELECT * and save the result as a JSON file
    for table in tables:
        print(table)
        table_name = table['Tables_in_defaultdb']
        cursor.execute(f"SELECT * FROM {table_name}")
        rows = cursor.fetchall()

        # Write data to a JSON file
        with open(f"data/{table_name}.json", 'w') as f:
            json.dump(rows, f, default=date_handler)

    connection.close()


def send_data():
    print('Sending data')
    files = os.listdir('data')
    filenames = [f.replace('.json', '') for f in files]
    connection = connect_to_db()
    cursor = connection.cursor()

    for filename in filenames:
        table_name = filename
        with open(f"data/{filename}.json", 'r', encoding='utf-8') as f:
            records = json.load(f)

        for record in records:
            placeholders = ', '.join(['%s'] * len(record))
            columns = ', '.join(record.keys())
            sql = f"INSERT INTO {table_name} ({columns}) VALUES ({placeholders}) ON DUPLICATE KEY UPDATE "
            updates = ', '.join(f'{k}=VALUES({k})' for k in record.keys())
            sql += updates
            cursor.execute(sql, list(record.values()))

    connection.commit()
    connection.close()


if __name__ == '__main__':
    if not os.path.exists('data'):
        print('Data directory does not exist')
        print('Creating data directory')
        os.makedirs('data')
        get_data()
    else:
        send_data()
