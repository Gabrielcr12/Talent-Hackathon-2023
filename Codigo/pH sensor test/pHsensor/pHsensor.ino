#include <Wire.h>
 
#define Sensor1Pin 0                // The pH meter Analog output is connected with the Arduino’s Analog
#define Sensor2Pin 1                // The pH meter Analog output is connected with the Arduino’s Analog
unsigned long int avgValue;         // Store the average value of the sensor feedback
int buf[10], temp;
 
void setup()
{
  Serial.begin(9600);  
  Serial.println("Ready"); 
}

void loop()
{
  float phValue = 0;

  // SENSOR 1

  for(int i = 0; i < 10; i++)       // Get 10 sample value from the sensor for smooth the value
  { 
    buf[i]=analogRead(Sensor1Pin);
    delay(10);
  }
  for(int i = 0; i < 9; i++)        // Sort the analog from small to large
  {
    for(int j = i + 1; j < 10; j++)
    {
      if(buf[i] > buf[j])
      {
        temp = buf[i];
        buf[i] = buf[j];
        buf[j] = temp;
      }
    }
  }
  
  avgValue = 0;
  
  for(int i = 2; i < 8; i++)                         // Take the average value of 6 center sample
  {
    avgValue += buf[i];
  }

  phValue = (float)avgValue * 5.0 / 1024 / 6;        // Convert the analog into millivolt

  Serial.print("Sensor 1 mV: ");  
  Serial.print(phValue); 
  Serial.println();

  phValue = 3.5 * phValue;                           // Convert the millivolt into pH value
  
  Serial.print("Sensor 1 pH: ");  
  Serial.print(phValue); 
  Serial.println();

  // SENSOR 2

  for(int i = 0; i < 10; i++)       // Get 10 sample value from the sensor for smooth the value
  { 
    buf[i]=analogRead(Sensor2Pin);
    delay(10);
  }
  for(int i = 0; i < 9; i++)        // Sort the analog from small to large
  {
    for(int j = i + 1; j < 10; j++)
    {
      if(buf[i] > buf[j])
      {
        temp = buf[i];
        buf[i] = buf[j];
        buf[j] = temp;
      }
    }
  }
  
  avgValue = 0;
  
  for(int i = 2; i < 8; i++)                         // Take the average value of 6 center sample
  {
    avgValue += buf[i];
  }
  
  phValue = (float)avgValue * 5.0 / 1024 / 6;        // Convert the analog into millivolt

  Serial.print("Sensor 2 mV: ");  
  Serial.print(phValue); 
  Serial.println();

  phValue = 3.5 * phValue;                           // Convert the millivolt into pH value
  
  Serial.print("Sensor 2 pH: ");  
  Serial.print(phValue); 
  Serial.println();

  delay(1000);
}