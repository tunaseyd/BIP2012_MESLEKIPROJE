import os
import win32com.client
import datetime

def create_task(program_path, script_path, task_name, start_time, interval_minutes=None):
    #tskschd ile bağlantı sağlanıp işleme hazır hake getirilir
    scheduler = win32com.client.Dispatch('Schedule.Service')
    scheduler.Connect()

    # Dosyaların kökü için get folder ile dosya alma komutu ekleniyor 
    root_folder = scheduler.GetFolder('\\')

    # Yeni bir işlem gerçekleşeceği için kayıt açıyoruz
    task_def = scheduler.NewTask(0)

    # Temel ayar seçeneklerini kod tabanında değiştiriyoruz ki başlangıçta çalıştırılmaya başlayabilsin...
    task_def.RegistrationInfo.Description = 'Scheduled Task for ' + task_name
    task_def.Settings.Enabled = True
    task_def.Settings.AllowDemandStart = True
    task_def.Settings.StartWhenAvailable = True

    # görevler daha doğrusu ".py" dosyalarını neyin tetiklediğini veriyoruz
    trigger = task_def.Triggers.Create(1)  # 1 represents daily trigger

    if start_time:
        #Zaman formatını ayarlayıp genel kullanıma geçirmek için ne zaman başlayacağını giriyoruz.
        if isinstance(start_time, str):
            start_time = datetime.datetime.strptime(start_time, "%H:%M")
            start_time = start_time.strftime("%Y-%m-%dT%H:%M:%S")
        trigger.StartBoundary = start_time

    if interval_minutes:
        trigger.Repetition.Interval = f'PT{interval_minutes}M'  # Kaç dakikalık döngü
        trigger.Repetition.Duration = 'PT24H'  # 24 saat boyunca tekrar et

    # Uygulama ile ne yapılacağını belirliyoruz
    action = task_def.Actions.Create(0)  # 0 burada programın çalışması gerektiği belirten seçenektir...
    action.Path = program_path
    action.Arguments = script_path

    # Root üzerinden işlemin dosyası depolanıyor
    task_folder = root_folder.GetFolder('\\')

    # İşlemleri diğerlerinin arasına ekleme işlemi yapılıp hangi alt işlemden geçiceğini belirliyoruz
    registered_task = task_folder.RegisterTaskDefinition(
        task_name,
        task_def,
        6,  # 6 yaratma veya güncelleme amacıyla kullanılır
        '', '',
        3  # 3 ise işaretlemek için
    )

    # Program işlemler arasında yer alıp almadığını inceliyoruz...
    if registered_task is not None:
        print(f'Task "{task_name}" registered successfully.')
    else:
        print(f'Failed to register task "{task_name}".')

# Python dil kökünü işaretliyoruz ki ".py" çalışırken okuyabilsinler
python_interpreter = r'C:\Python\python.exe'

script_dir = os.path.dirname(os.path.abspath(__file__))

# işlemlerin "scriptlerinin" dosya konumu işaretlenir
RTappLoc = os.path.join(script_dir, 'RTapp.py')
saveLoc = os.path.join(script_dir, 'save.py')

# Anlık zamanı belirliyoruz
current_time = datetime.datetime.now().strftime("%H:%M")

# zamanlamaların yerleşmesi amacıyla zamanlama ve temel bilgiler yerleştirilir
create_task(python_interpreter, RTappLoc, 'RTapp Task', current_time, 15)
create_task(python_interpreter, saveLoc, 'Save Task', '08:00')

print('Scheduled tasks creation completed.')
