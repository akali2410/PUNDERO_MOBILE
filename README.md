# PUNDERO Mobile App – Real-Time Driver Interface

This is the **driver-facing mobile app** for the PUNDERO system — a smart delivery management platform for small distribution companies.

Built with **.NET MAUI**, the app enables drivers to view delivery invoices, track routes, and update delivery status — all with integrated GPS tracking and map visualization.

🎥 **Demo Video**: [Watch here](https://vimeo.com/1105660593)

---

## 📱 Features

- View assigned delivery invoices and details
- Real-time location tracking using device GPS
- Integrated map with optimized delivery route (Google Maps API)
- Mark deliveries as completed

---

## 🛠 Tech Stack

- .NET MAUI (C#)
- REST API Integration (.NET Core Web API)
- Google Maps API
- Xamarin.Essentials (for geolocation)
- Responsive design for Android devices

---

## 🔗 Related Repositories

- **Frontend (React JS)**  
  [azradaut/PUNDERO-FE](https://github.com/azradaut/PUNDERO-FE)

- **Backend (.NET Core Web API)**  
  [akali2410/PUNDERO](https://github.com/akali2410/PUNDERO)

- **Demo Video**  
  [https://vimeo.com/1105660593](https://vimeo.com/1105660593)

---

## 🧪 API Interactions

- `GET /api/driver/invoices`
- `POST /api/location/update`
- `PUT /api/invoice/complete`

---

## 🚀 Setup Instructions

1. Clone the repository
2. Set up device permissions (location, internet)
3. Configure base API URL in code
4. Deploy to Android device/emulator via Visual Studio

---


