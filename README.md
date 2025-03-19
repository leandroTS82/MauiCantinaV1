# CantinaV1

Arquitetura inicial:
CantinaV1/
│-- CantinaV1.csproj
│-- App.xaml
│-- MainPage.xaml
│-- MainPage.xaml.cs
│-- Platforms/
│-- Resources/
│-- Data/ (criar)
│-- Models/ (criar)


Para gerar o **APK** do seu aplicativo .NET MAUI para Android, siga estes passos:  

---

### **1. Configurar o Projeto para Release**
Primeiro, configure o projeto para gerar um build de **Release**. No Visual Studio:  
- Vá até **Build** → **Configuration Manager**  
- Selecione **Release** no dropdown de configuração  

Ou no terminal, rode:  
```sh
dotnet build -c Release -f net8.0-android
```

---

### **2. Gerar o APK**
Agora, use o seguinte comando para compilar e gerar o APK:  
```sh
dotnet publish -c Release -f net8.0-android --no-build
```
Isso criará o APK na pasta:  
```
CantinaV1\bin\Release\net8.0-android\publish\
```
O arquivo gerado terá um nome semelhante a **CantinaV1-Signed.apk**.