import os
import datetime
import pandas as pd
import sqlite3
import getpass

url = 'ProjectTest.db'

def main():
    # Kullanıcının ismi ile kök dosyasını bul
    user_folder = os.path.expanduser("~")

    # 'Logs' dosyasının varlığı be konumu değerlendirilmesinde kullanılmak üzere logs_folder değerine atanır
    logs_folder = os.path.join(user_folder, "Logs")

    # logs_folder değeri yoksa yeni bir dosya yarat
    os.makedirs(logs_folder, exist_ok=True)

    # Tarih damgası için değeri topla
    today = datetime.date.today()
    Date = today.strftime("%Y-%m-%d")

    # Damganın uygulandığı bir dosya oluştur
    Filename = f"Old_Log_{Date}.xlsx"

    # Database'e bağlantı
    conn = sqlite3.connect(url)
    query = "SELECT * FROM RTS"
    df = pd.read_sql_query(query, conn)

    # Kaydet
    FilePath = os.path.join(logs_folder, Filename)
    df.to_excel(FilePath, index=False)

    # Tabloyu temizle
    clear_query = "DELETE FROM RTS"
    conn.execute(clear_query)
    conn.commit()
    conn.close()

    print("Dosyalar", FilePath,"'a kaydedildi")
    print("Tablo sıfırlandı.")


main()
