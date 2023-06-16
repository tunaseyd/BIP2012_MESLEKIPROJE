import psutil
import sqlite3
#Database bağlantısını gerçekleştirmek için gerekli komut dizini
#r'' ile yaıyı 'raw' forma getirerek string'in decode sisteminden ayrılmış şkelide okunmasını
url = 'ProjectTest.db'
#Başlangıç ve Test amaçlı....
def CreateTable():
    conn = sqlite3.connect(url)
    cursor = conn.cursor()

    #Tablo kontrol et ve yoksa yeni oluştur...
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS RTS (
            pid INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT,
            path TEXT,
            time INTEGER
        )
    ''')

    conn.commit()
    conn.close()

def SaveRunningApps(apps):
    conn = sqlite3.connect(url)
    cursor = conn.cursor()

    for app in apps:
        # Uygulama database'de varmı diye kontrol et
        cursor.execute('SELECT COUNT(*) FROM RTS WHERE name = ?', (app['name'],))
        count = cursor.fetchone()[0]

        if count > 0:
            # eğer database'de aynı uygulama varsa son güncellenen zamana bakılacak
            cursor.execute('UPDATE RTS SET time = time + 0.25 WHERE name = ?', (app['name'],))
        else:
            # eğer uygulama yeni eklenmiş ise direkt olarak 0 ile başlayacak
            cursor.execute('INSERT INTO RTS (pid, name, path, time) VALUES (?, ?, ?, ?)',
                           (app['pid'], app['name'], app['exe'], 0))
        

    conn.commit()
    conn.close()

def getRunningApps():
    apps = []
    #Sürekli görünme riski olan ve hizmet olarak tasarlanmış çalıştırılabilir uygulamaların kalabalık yapmaması adına patika filtresi..
    PathsToExclude = ['\\Windows\\', '\\Program Files\\', '\\Windows Defender\\', 'Update']
    AppNames = set()
    #if filtreleri ile ekstra eklemeler yap
    for proc in psutil.process_iter(['pid', 'name', 'exe']):
        if proc.info['name'].endswith('.exe') and not any(path in proc.info['exe'] for path in PathsToExclude):
            if 'CrashHandler' and 'Service' not in proc.info['name']:
                if proc.info['name'] not in AppNames:
                    AppNames.add(proc.info['name'])
                    apps.append(proc.info)
    return apps

def main():
    runningApps = getRunningApps()
    CreateTable()
    SaveRunningApps(runningApps)
    print("Çalışan uygulamalar başarıyla Veritabanına gönderildi.")

main()
