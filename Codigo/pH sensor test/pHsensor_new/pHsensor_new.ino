#define SINGLE
//#define BUFFER

void setup() 
{ 
  Serial.begin(9600);
} 
 
void loop() 
{ 
  int pHsensorvalue = 0;
  
  #ifdef SINGLE
  pHsensorvalue = analogRead(A0);    
  #endif

  #ifdef BUFFER
  for (int i = 0; i < 10; i++)
  {
    pHsensorvalue += analogRead(A0);
    delay(10);
  }
  pHsensorvalue = pHsensorvalue / 10;
  #endif
  
  float pHvolt = pHsensorvalue * (5.0/1024);
  float pH = -3.5*pHvolt+21.34-0.7;
  
  //Serial.println(pHsensorvalue); 
  Serial.println(pHvolt); 
  //Serial.println(pH); 

  delay(250);
}