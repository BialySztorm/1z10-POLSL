import json
import pymysql
import os


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


def get_data(table_name):
    connection = connect_to_db()
    cursor = connection.cursor()
    if (table_name):
        cursor.execute(f"SELECT * FROM {table_name}")
        rows = cursor.fetchall()
        return rows
    else:
        cursor.execute("SHOW TABLES")
        tables = cursor.fetchall()
        table_data = {}
        for table in tables:
            table_name = table['Tables_in_defaultdb']
            cursor.execute(f"SELECT * FROM {table_name}")
            rows = cursor.fetchall()
            table_data[table_name] = rows
        return table_data


def get_table_names():
    connection = connect_to_db()
    cursor = connection.cursor()
    cursor.execute("SHOW TABLES")
    tables = cursor.fetchall()
    table_names = [table['Tables_in_defaultdb'] for table in tables]
    table_names = {str(i + 1): table_name for i, table_name in enumerate(table_names)}
    return table_names


def show_data():
    data = get_data(None)
    for table_name, rows in data.items():
        print(f"Table: {table_name}")
        for row in rows:
            print(row)
        print()


def show_table_data():
    table_names = get_table_names()
    print('Tables:', table_names)
    option = input('Choose table number: ')
    data = get_data(table_names[option])
    for row in data:

        print(row)


def build_sql_query(table_name, base_sql=""):
    # Pobierz relacje dla tabeli
    with open(f'relations/{table_name}.json', 'r') as file:
        relations = json.load(file)

    # Zbuduj zapytanie SQL, które łączy tabelę z wszystkimi powiązanymi tabelami
    sql = base_sql or f'SELECT * FROM "{table_name}"'
    for relation in relations:
        sql += f' INNER JOIN "{relation["to_table"]}" ON "{table_name}"."{relation["from_column"]}" = "{relation["to_table"]}"."{relation["to_column"]}"'
        sql = build_sql_query(relation["to_table"], sql)

    return sql


def show_data_with_related_tables(table_name):
    connection = connect_to_db()
    cursor = connection.cursor()

    # Zbuduj zapytanie SQL
    sql = build_sql_query(table_name)

    # print(sql)
    # Wykonaj zapytanie i wyświetl wyniki
    cursor.execute(sql)
    rows = cursor.fetchall()
    for row in rows:
        print(row)

    connection.close()


def menu():
    print('1. Show all data')
    print('2. Show data for a specific table')
    print('3. Show Expanded question data')
    print('4. Show Expanded highscores data')
    print('0. Exit')
    choice = input('Enter choice: ')
    if choice == '1':
        show_data()
    elif choice == '2':
        show_table_data()
    elif choice == '3':
        show_data_with_related_tables('question')
        # show_special_data("SELECT Questions.idQuestion, QuestionsTypes.QuestionType, Questions.question, Questions.answer FROM Questions INNER JOIN QuestionsTypes ON Questions.type = QuestionsTypes.idQuestionType;")
    elif choice == '4':
        # show_special_data("SELECT Highscores.idHighscores, Highscores.highscore, Players.firstName, Players.lastName, Players.age, Games.date FROM Highscores INNER JOIN Players ON Highscores.player = Players.idPlayers INNER JOIN Games ON Players.gameNumber = Games.idGame;")
        show_data_with_related_tables('highscore')
    elif choice == '0':
        exit()
    else:
        print('Invalid choice')
    os.system('pause')
    os.system('cls')


if __name__ == '__main__':
    while True:
        menu()
