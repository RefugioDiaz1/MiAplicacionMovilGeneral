# MobileIntegrationApp

Aplicación móvil multiplataforma desarrollada con **.NET MAUI**, orientada a la integración de servicios empresariales existentes y al consumo de datos operativos desde sistemas internos.

La aplicación fue diseñada para funcionar en **Android e iOS**, incorporando validaciones de seguridad, conexión a APIs propias y consumo de Web Services empresariales.

---

## 🧠 Descripción general

**MobileIntegrationApp** es una aplicación móvil creada como cliente empresarial que permite a los usuarios acceder a información interna de forma segura desde dispositivos móviles.

El proyecto integra distintos servicios backend y funcionalidades móviles, actuando como un puente entre aplicaciones modernas y sistemas empresariales ya existentes.

---

## 📱 Plataforma y tecnología

- **.NET MAUI**
- **C#**
- **XAML**
- **Android**
- **iOS**

---

## 🔐 Seguridad y control de acceso

La aplicación implementa controles básicos de seguridad a nivel dispositivo:

- 📱 **Validación de identificador del dispositivo**
  - Solo dispositivos previamente registrados en la base de datos pueden utilizar la aplicación.
- 🔒 **Acceso controlado**
  - El uso de la app está limitado a dispositivos autorizados.
- 🌐 **Consumo de API segura**
  - La información de contacto del usuario se guarda mediante una API conectada a SQL Server.

---

## 🔗 Integraciones backend

### 🧩 API propia
- Registro y almacenamiento de datos de contacto del usuario.
- Persistencia de información en **SQL Server**.
- Validación de dispositivos autorizados.

### 🧩 Web Service empresarial existente
- Integración con un Web Service desarrollado en **Java / JSP**.
- Comunicación desde la app móvil hacia el servicio.
- El Web Service consulta una base de datos **Universe**.
- Retorno de información de refacciones en tiempo real.

---

## 🧾 Funcionalidad de cotizador de refacciones

- 🔍 Búsqueda de refacciones desde la aplicación móvil.
- 📡 Consulta directa al Web Service empresarial.
- 🗄️ Obtención de datos desde base Universe.
- 📱 Visualización de resultados en la app móvil.

Esta funcionalidad permitió reutilizar sistemas existentes sin necesidad de migrarlos.

---

## 📸 Funcionalidades adicionales (pruebas / prototipos)

- 📄 **Escaneo de documentos**
- 🖼️ **Conversión de imágenes a PDF**
- 🧪 Pantallas de prueba para futuras funcionalidades

Estas funcionalidades se implementaron como pruebas de concepto para evaluar capacidades del dispositivo.

---

## 📦 Distribución de la aplicación

### 🤖 Android
- Generación de archivo **APK**
- Instalación directa en dispositivos Android

### 🍎 iOS
- Generación de archivo **IPA**
- Distribución controlada mediante **UUID de dispositivos**
- Instalación solo en dispositivos autorizados desde el portal de Apple

---

## 🧠 Estado del proyecto

- ✅ Funcional en Android e iOS
- ✅ Integración exitosa con servicios empresariales
- 🔄 En desarrollo / evolución
- 📌 Algunas funcionalidades quedaron como prototipo (unidades nuevas, ampliaciones futuras)

---

## 🧠 Aprendizajes clave del proyecto

- Desarrollo móvil multiplataforma con .NET MAUI
- Integración con APIs y Web Services existentes
- Comunicación entre tecnologías distintas (.NET ↔ Java)
- Control de acceso por dispositivo
- Generación y distribución de builds móviles
- Manejo de flujos empresariales reales

---

## 👤 Autor

Desarrollado por **Refugio Díaz**  
Desarrollador de Software | Backend & Soluciones de Datos
