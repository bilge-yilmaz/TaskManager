# Task Manager - Görev Yöneticisi

Modern ve kullanıcı dostu bir görev yönetim uygulaması. ASP.NET Core Web API ve MongoDB ile geliştirilmiştir.

## Özellikler

- **JWT Authentication** - Güvenli kullanıcı girişi ve kayıt sistemi
- **Görev Yönetimi** - Görev ekleme, düzenleme, silme ve tamamlama işlemleri
- **Kategoriler** - Görevleri kategorilere ayırma (Kişisel, İş, Sağlık, Eğitim vb.)
- **Öncelik Seviyeleri** - Düşük, Orta, Yüksek, Kritik öncelik seviyeleri
- **Filtreleme ve Arama** - Görevleri duruma, önceliğe ve kategoriye göre filtreleme
- **Dashboard** - Görev istatistikleri ve özet bilgiler
- **Etiketler** - Görevlere özel etiketler ekleme
- **Responsive Tasarım** - Mobil ve masaüstü uyumlu arayüz
- **Sayfalama** - Büyük görev listelerini verimli görüntüleme

## Teknolojiler

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **MongoDB** - NoSQL veritabanı
- **JWT** - Authentication ve Authorization
- **Swagger** - API dokümantasyonu
- **BCrypt** - Şifre hashleme

### Frontend
- **HTML5 & CSS3** - Modern web standartları
- **JavaScript (ES6+)** - İnteraktif kullanıcı arayüzü
- **Bootstrap 5** - Responsive UI framework
- **Font Awesome** - İkon kütüphanesi

## Kurulum

### Gereksinimler
- .NET 8.0 SDK
- MongoDB Atlas hesabı (veya yerel MongoDB kurulumu)
- Modern web tarayıcısı

### Kurulum Adımları

1. **Projeyi klonlayın**
   ```bash
   git clone https://github.com/yourusername/task-manager.git
   cd task-manager
   ```

2. **Bağımlılıkları yükleyin**
   ```bash
   dotnet restore
   ```

3. **MongoDB bağlantısını yapılandırın**
   
   `appsettings.json.example` dosyasını `appsettings.json` olarak kopyalayın ve MongoDB connection string'ini güncelleyin:
   ```bash
   cp appsettings.json.example appsettings.json
   ```
   
   Sonra `appsettings.json` dosyasında şu değerleri güncelleyin:
   ```json
   {
     "ConnectionStrings": {
       "MongoDB": "your-mongodb-atlas-connection-string"
     },
     "JwtSettings": {
       "SecretKey": "your-secure-secret-key-minimum-32-characters"
     }
   }
   ```

4. **Uygulamayı çalıştırın**
   ```bash
   dotnet run
   ```

5. **Tarayıcıda açın**
   
   http://localhost:5027 adresine gidin

## API Dokümantasyonu

Uygulama çalışırken Swagger UI'ya erişebilirsiniz:
- **Swagger UI**: http://localhost:5027/swagger

### Ana Endpoint'ler

#### Authentication
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/login` - Kullanıcı girişi
- `GET /api/auth/validate-token` - Token doğrulama

#### Tasks
- `GET /api/tasks` - Görevleri listele (filtreleme ve sayfalama ile)
- `POST /api/tasks` - Yeni görev oluştur
- `GET /api/tasks/{id}` - Görev detayını getir
- `PUT /api/tasks/{id}` - Görev güncelle
- `DELETE /api/tasks/{id}` - Görev sil
- `POST /api/tasks/{id}/complete` - Görevi tamamla
- `POST /api/tasks/{id}/uncomplete` - Görev tamamlanmadı işaretle
- `GET /api/tasks/stats` - Görev istatistikleri

## Kullanım

### Kayıt ve Giriş
1. Ana sayfada "Kayıt Ol" butonuna tıklayın
2. Gerekli bilgileri doldurun
3. Kayıt sonrası otomatik giriş yapılır

### Görev Yönetimi
1. **Görev Ekleme**: "Yeni Görev Ekle" formunu kullanın
2. **Görev Düzenleme**: Görev kartındaki "Düzenle" butonuna tıklayın
3. **Görev Tamamlama**: "Tamamla" butonuna tıklayın
4. **Görev Silme**: "Sil" butonuna tıklayın

### Filtreleme
- Durum, öncelik ve kategoriye göre filtreleyin
- Arama kutusunu kullanarak görevlerde arama yapın
- "Temizle" butonu ile filtreleri sıfırlayın

## Proje Yapısı

```
TaskManager/
├── Controllers/          # API Controllers
├── Models/              # Data Models ve DTOs
├── Services/            # Business Logic
├── wwwroot/            # Static Files
│   ├── css/            # Stil dosyaları
│   ├── js/             # JavaScript dosyaları
│   └── index.html      # Ana sayfa
├── appsettings.json    # Konfigürasyon
└── Program.cs          # Uygulama giriş noktası
```

## Konfigürasyon

### MongoDB Ayarları
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb+srv://username:password@cluster.mongodb.net/database-name"
  },
  "DatabaseSettings": {
    "DatabaseName": "task-manager",
    "UsersCollectionName": "user",
    "TasksCollectionName": "taskitem"
  }
}
```

### JWT Ayarları
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "TaskManager",
    "Audience": "TaskManagerUsers",
    "ExpiryInHours": 24
  }
}
```

## Katkıda Bulunma

1. Bu repository'yi fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

