# XmlXsdValidator (WinForms)

`XmlXsdValidator`, bir **Windows Forms (.NET 8)** masaüstü uygulamasıdır. Seçtiğiniz bir **XML** dosyasını iki farklı şemaya göre doğrulamak ve (isteğe bağlı) XML’i **hiyerarşik bir grid** üzerinde düzenleyip **XSD kısıtlarına göre anlık doğrulama** almak için geliştirilmiştir.

Uygulama özellikle XML içinde:
- **Header / `AppHdr`** bölümünü (Header XSD)
- **Document / `Document`** bölümünü (Document XSD)

ayrı ayrı doğrulama yaklaşımını kullanır.

## Özellikler

- **XML görüntüleme**: Seçilen XML, sol tarafta okunabilir biçimde gösterilir.
- **Çift şema ile doğrulama**:
  - `AppHdr` için Header XSD
  - `Document` için Document XSD
- **XML editör (TreeGrid)**:
  - XML’i editöre yükleyip alanları tablo üzerinde düzenleyebilirsiniz.
  - Parent/child hiyerarşisi expand/collapse ile yönetilir.
- **XSD’den kısıt çıkarımı ve validasyon**:
  - `pattern` (regex), `minLength`, `maxLength`
  - sayısal limitler (`minInclusive`, `maxInclusive`, vb.)
  - `enumeration` değerleri (enum alanlar için seçim kutusu)
- **Hata odaklı UX**:
  - Geçersiz alanlar grid’de görsel olarak işaretlenir.
  - Sağ tık menüsünden validasyon özeti ve filtreleme seçenekleri kullanılabilir.

## Gereksinimler

- **Windows** (proje `net8.0-windows` ve WinForms)
- **.NET 8 SDK**
- (Önerilen) **Visual Studio 2022** (WinForms designer için)

## Çalıştırma

### Visual Studio ile

1. `deneme.sln` dosyasını açın
2. Startup project: `validator`
3. Çalıştırın (F5)

### Komut satırı ile

```bash
dotnet --version
dotnet run --project deneme/validator.csproj
```

> Not: WinForms olduğu için uygulama Windows üzerinde çalışır.

## Kullanım

1. **XML** butonundan doğrulanacak `.xml` dosyasını seçin
2. **Header XSD** butonundan `AppHdr` için `.xsd` dosyasını seçin
3. **Document XSD** butonundan `Document` için `.xsd` dosyasını seçin
4. **Doğrula**’ya tıklayın  
   - Sonuçlar alttaki “Sonuçlar” bölümüne yazılır
5. (İsteğe bağlı) **XML Editöre Yükle** ile editörü açın ve alanları düzenleyin
6. **Kaydet** ile değişiklikleri bellekte güncelleyin
7. **Güncelenen Xml’i Gör** ile güncellenmiş XML’i sol tarafta görüntüleyin

## Kısayollar ve editör ipuçları

Editör (grid) üzerinde:
- **Sağ tık**: Menü (Expand/Collapse, validasyon özeti, filtreleme)
- **Ctrl + +**: Tümünü genişlet
- **Ctrl + -**: Tümünü daralt
- **Ctrl + V**: Validasyon özeti
- **Space**: Seçili satırda expand/collapse

## Proje yapısı

- `deneme.sln`: Visual Studio çözümü
- `deneme/validator.csproj`: WinForms proje dosyası
- `deneme/Form1.*`: UI ve olay bağlama
- `deneme/UI/`: Form event akışları
- `deneme/Operations/`: Grid/Xml operasyonları
- `deneme/Validation/`: XSD kısıtlarına göre validasyon
- `deneme/XsdSchemaAnalyzer.cs`: XSD’yi analiz edip kısıt çıkarımı

## Notlar / Sınırlamalar

- Uygulama **Header** için XML içinde `AppHdr`, **Document** için `Document` elementini arar.
- Boş değerler çoğu kontrolde “geçerli” kabul edilir (zorunlu alan kontrolü şemaya/kurala göre genişletilebilir).

