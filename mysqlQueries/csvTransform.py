import os
import json
import csv


def json_to_csv():
    print('Converting JSON to CSV')
    files = [f for f in os.listdir('data') if f.startswith('question') and f.endswith('.json')]
    filenames = [f.replace('.json', '') for f in files]
    for filename in filenames:
        with open(f"data/{filename}.json", 'r') as f:
            data = json.load(f)
            keys = data[0].keys()
            with open(f"data/{filename}.csv", 'w', newline='') as f:
                writer = csv.DictWriter(f, fieldnames=keys)
                writer.writeheader()
                for row in data:
                    writer.writerow(row)


def csv_to_json():
    print('Converting CSV to JSON')
    files = [f for f in os.listdir('data') if f.startswith('question') and f.endswith('.csv')]
    filenames = [f.replace('.csv', '') for f in files]
    for filename in filenames:
        with open(f"data/{filename}.csv", 'r') as f:
            reader = csv.DictReader(f)
            rows = list(reader)
            with open(f"data/{filename}.json", 'w') as f:
                json.dump(rows, f)


if __name__ == '__main__':
    if not os.path.exists('data'):
        print('Data directory does not exist')
    option = input('Enter 1 to convert JSON to CSV, 2 to convert CSV to JSON: ')
    if option == '1':
        json_to_csv()
    elif option == '2':
        csv_to_json()
    else:
        print('Invalid option')
