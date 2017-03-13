#include <Adafruit_NeoPixel.h>

#define sizeofarray( a ) ( sizeof(a) / sizeof((a)[0]) )

#define LED_PIN       10
#define PLAYER_COUNT  4

Adafruit_NeoPixel Leds = Adafruit_NeoPixel(12, LED_PIN, NEO_RGB + NEO_KHZ800);

//  gr: one of the pins broke, so OOO :/
int PlayerLeds[PLAYER_COUNT][3] = 
{
  0,1,2,
  3,4,5,
  6,7,8,
  11,9,10
};

int PlayerJoystickPins[PLAYER_COUNT][2] = 
{
  4,5,
  2,3,
  6,7,
  9,8,
};

uint32_t PlayerColours[PLAYER_COUNT] = 
{
  Leds.Color( 255,0,0 ),
  Leds.Color( 0,255,255 ),
  Leds.Color( 255,255,0 ),
  Leds.Color( 255,0,255 ),
};

void setup() 
{
  Leds.begin();
  Leds.show();

int* AllPins = PlayerJoystickPins[0];
  for ( int i=0;  i<sizeofarray(PlayerJoystickPins);  i++ )
    pinMode(AllPins[i], INPUT);
}


namespace Direction
{
  enum TYPE
  {
    Left,
    None,
    Right
  };
}

Direction::TYPE PlayerDirection[PLAYER_COUNT] = 
{
  Direction::None,
  Direction::None,
  Direction::None,
  Direction::None,
};

uint32_t ColourOff = Leds.Color( 0,0,0,0 );


int FlashCount = 0;
int FlashAlternate = 20;
int RefreshRate = 50;
void loop() 
{
  /*
  for ( int p=0;  p<PLAYER_COUNT; p++ )
  {
    auto PinLeft = PlayerJoystickPins[p][0];
    auto PinRight = PlayerJoystickPins[p][1];
    auto LeftDown = digitalRead( PinLeft ) == HIGH;
    auto RightDown = digitalRead( PinRight ) == HIGH;
    PlayerDirection[p] = LeftDown ? Direction::Left : RightDown ? Direction::Right : Direction::None;
  }
  
  for ( int p=0;  p<PLAYER_COUNT; p++ )
  {
    auto PlayerColour = PlayerColours[p];
    
    Leds.setPixelColor( PlayerLeds[p][0], PlayerDirection[p]==Direction::Left ? PlayerColour : ColourOff );
    Leds.setPixelColor( PlayerLeds[p][1], PlayerColour );
    Leds.setPixelColor( PlayerLeds[p][2], PlayerDirection[p]==Direction::Right ? PlayerColour : ColourOff );
  }
  */

  FlashCount++;
  
  for ( int p=0;  p<PLAYER_COUNT; p++ )
  {
    auto PlayerColour = PlayerColours[p];
    auto PinLeft = PlayerJoystickPins[p][0];
    auto PinRight = PlayerJoystickPins[p][1];
    auto LeftDown = digitalRead( PinLeft ) == HIGH;
    auto RightDown = digitalRead( PinRight ) == HIGH;

    auto Flash = (FlashCount%FlashAlternate) <= (FlashAlternate/2);
    
    Leds.setPixelColor( PlayerLeds[p][0], LeftDown ? PlayerColour : ColourOff );
    Leds.setPixelColor( PlayerLeds[p][1], Flash ? PlayerColour : ColourOff );
    Leds.setPixelColor( PlayerLeds[p][2], RightDown ? PlayerColour : ColourOff );
  }

  /*
  PlayerOffset++;
  for ( int i=0;  i<12; i++ )
  {
    int* LedIndexes = PlayerLeds[0];
    if ( i == (PlayerOffset % 12 ) )
      Leds.setPixelColor( LedIndexes[i], PlayerColours[0] );
    else
      Leds.setPixelColor( LedIndexes[i], ColourOff );
  }
  */
  Leds.show();
  delay( 1000 / RefreshRate );
  
  
}




