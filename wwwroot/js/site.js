let currentUnit = 'C';
let map;
let marker;

function initMap() {
    const initialPosition = { lat: 59.3293, lng: 18.0686 }; // Stockholm som standard
    map = new google.maps.Map(document.getElementById("map"), {
        center: initialPosition,
        zoom: 10,
    });

    marker = new google.maps.Marker({
        position: initialPosition,
        map: map,
    });
}

function changeUnit(unit) {
    if (unit === currentUnit) return; // Kontrollera om det redan är rätt enhet
    currentUnit = unit;
    convertDisplayedTemperatures(unit);
}

function convertTemperature(tempC, toUnit) {
    return toUnit === 'F' ? (tempC * 9 / 5 + 32).toFixed(1) : tempC;
}

function convertDisplayedTemperatures(toUnit) {
    // Uppdatera den aktuella temperaturen högst upp
    const temperatureElement = document.getElementById('temperature');
    let originalTempC = parseFloat(temperatureElement.getAttribute('data-original-temp'));
    if (!isNaN(originalTempC)) {
        const convertedTemp = convertTemperature(originalTempC, toUnit);
        temperatureElement.textContent = `${convertedTemp} °${toUnit}`;
    }

    // Uppdatera temperaturerna i tabellen
    const tempElements = document.querySelectorAll('#hourly-temperature-row .temp');
    tempElements.forEach(element => {
        let originalTemp = parseFloat(element.getAttribute('data-original-temp'));
        if (!isNaN(originalTemp)) {
            element.textContent = `${convertTemperature(originalTemp, toUnit)} °${toUnit}`;
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("locationSelect").addEventListener("change", function () {
        const location = this.value;
        updateMap(location);
    });

    document.getElementById("unitSelect").addEventListener("change", function () {
        const selectedUnit = this.value;
        changeUnit(selectedUnit); // Uppdaterar temperaturer direkt
    });

    document.getElementById("getTemperature").addEventListener("click", function () {
        const selectedDate = document.getElementById("dateSelect").value;
        const location = document.getElementById("locationSelect").value;
    
        if (selectedDate && location) {
            document.getElementById("forecast-date").textContent = `Datum: ${selectedDate}`;
    
            fetch(`/Home/GetTemperatureForSelectedDate?location=${location}&date=${selectedDate}`)
                .then(response => response.text())
                .then(data => {
                    const hourlyDataArray = data.split(", ");
                    const firstHourData = hourlyDataArray[0]; // Exempel: Använd bara första timmen för att visa
    
                    const [timeTempWind, iconClass] = firstHourData.split("|");
                    const [time, temperature, windSpeed, windDirection] = timeTempWind.split(", ");

                    console.log("Temperature:", temperature);
                    console.log("Wind Speed:", windSpeed);
                    console.log("Wind Direction:", windDirection);
    
                    // Uppdatera HTML-elementen
                    document.getElementById("weather-icon").className = `wi ${iconClass.trim()}`;
                    document.getElementById("hour-time").textContent = new Date(time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                    document.getElementById("temperature-value").textContent = `${parseFloat(temperature).toFixed(1)} °${currentUnit}`;
                    document.getElementById("wind-speed").textContent = `Wind Speed: ${windSpeed} m/s`;
                    document.getElementById("wind-direction").textContent = `Wind Direction: ${windDirection}°`;
                })
                .catch(error => console.error("Error fetching temperature data:", error));
        } else {
            alert("Vänligen välj ett datum och en plats.");
        }
    });
});

function updateMap(location) {
    const coordinates = {
        stockholm: { lat: 59.3293, lng: 18.0686 },
        gothenburg: { lat: 57.7089, lng: 11.9746 },
        malmo: { lat: 55.6050, lng: 13.0038 },
        uppsala: { lat: 59.8586, lng: 17.6389 },
        vasteras: { lat: 59.6162, lng: 16.5528 },
        orebro: { lat: 59.2741, lng: 15.2066 },
        helsingborg: { lat: 56.0465, lng: 12.6945 },
        jonkoping: { lat: 57.7815, lng: 14.1562 },
        norrkoping: { lat: 58.5877, lng: 16.1924 },
        lund: { lat: 55.7047, lng: 13.1910 },
        umea: { lat: 63.8258, lng: 20.2630 },
        gavle: { lat: 60.6749, lng: 17.1413 },
        kiruna: { lat: 67.8557, lng: 20.2253 },
        ornskoldsvik: { lat: 63.2909, lng: 18.7152 }
    };

    if (coordinates[location]) {
        const newPosition = coordinates[location];
        map.setCenter(newPosition);
        marker.setPosition(newPosition);

        fetch(`/Home/GetWeatherTemperatureNow?location=${location}`)
            .then(response => response.text())
            .then(data => {
                const [temperature, iconClass] = data.split('|');
                document.querySelector('#temperature').setAttribute('data-original-temp', temperature); // Spara originalvärdet
                document.querySelector('#temperature').textContent = `${convertTemperature(parseFloat(temperature), currentUnit)} °${currentUnit}`;
                document.querySelector('#weather-icon').className = `wi ${iconClass}`;
            })
            .catch(error => console.error('Error fetching current temperature:', error));
    }
}