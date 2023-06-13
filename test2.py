import psutil
import sqlite3
#Database bağlantısını gerçekleştirmek için gerekli komut dizini
def create_database():
    conn = sqlite3.connect('RTS.db')
    cursor = conn.cursor()

    #Tablo kontrol et ve yoksa yeni oluştur...
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS RTS (
            pid INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT,
            path TEXT,
            time INTEGER,
        )
    ''')

    conn.commit()
    conn.close()

def save_running_apps(apps):
    conn = sqlite3.connect('RTS.db')
    cursor = conn.cursor()

    # Her dosya veri tabanına eklenir...
    for app in apps:
        cursor.execute('INSERT INTO running_apps (pid, name, path, time) VALUES (?, ?, ?,?)',
                       (app['pid'], app['name'], app['exe'] ))

    conn.commit()
    conn.close()

def getRunningApps():
    apps = []
    #Temel dosya patika filtreleri, kalabalığı engeller...
    PathsToExclude = ['\\Windows\\', '\\Program Files\\', '\\AppData\\', '\\Windows Defender\\']
    AppNames = set()

    for proc in psutil.process_iter(['pid', 'name', 'exe']):
        #if loopları ile ekstra filtreleme uygulanır ve çıkan isimler daha sade kalır...
        if proc.info['name'].endswith('.exe') and not any(path in proc.info['exe'] for path in PathsToExclude):
            if 'CrashHandler' and 'Update' not in proc.info['name']:
                if proc.info['name'] not in AppNames:
                    AppNames.add(proc.info['name'])
                    apps.append(proc.info)
    return apps

def main():
    running_apps = getRunningApps()
    create_database()
    save_running_apps(running_apps)
    print("Running application information saved to the database.")

main()
