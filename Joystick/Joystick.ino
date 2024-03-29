//  https://github.com/MHeironimus/ArduinoJoystickLibrary/
#include <Joystick.h>

#include <Adafruit_NeoPixel.h>

#define sizeofarray( a ) ( sizeof(a) / sizeof((a)[0]) )

#define LED_PIN       10
#define PLAYER_COUNT  4
#define PAD_COUNT     PLAYER_COUNT
#define LED_BRIGHTNESS  20


bool TestFlash = false;
int FlashCount = 0;
int FlashAlternate = 20;
int RefreshRate = 50;

Adafruit_NeoPixel Leds = Adafruit_NeoPixel(12, LED_PIN, NEO_RGB + NEO_KHZ800);

uint32_t ColourOff = Leds.Color( 0,0,0,0 );

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

int GamepadIndex[PLAYER_COUNT] = 
{
  3,2,1,0
};

uint32_t PlayerColours[PLAYER_COUNT] = 
{
  Leds.Color( LED_BRIGHTNESS,0,LED_BRIGHTNESS ),
  Leds.Color( LED_BRIGHTNESS,LED_BRIGHTNESS,0 ),
  Leds.Color( 0,LED_BRIGHTNESS,LED_BRIGHTNESS ),
  Leds.Color( LED_BRIGHTNESS,0,0 ),
};

#define JOYSTICK_BUTTON_COUNT 0
#define ENABLE_Y_AXIS false

Joystick_ Joystick[PAD_COUNT] = {
  Joystick_(JOYSTICK_DEFAULT_REPORT_ID, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, ENABLE_Y_AXIS, false, false, false, false, false, false, false, false, false),
  Joystick_(JOYSTICK_DEFAULT_REPORT_ID+1, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, ENABLE_Y_AXIS, false, false, false, false, false, false, false, false, false),
  Joystick_(JOYSTICK_DEFAULT_REPORT_ID+2, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, ENABLE_Y_AXIS, false, false, false, false, false, false, false, false, false),
  Joystick_(JOYSTICK_DEFAULT_REPORT_ID+3, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, ENABLE_Y_AXIS, false, false, false, false, false, false, false, false, false),
};

#define AXIS_MIN  -1023
#define AXIS_MID  0
#define AXIS_MAX  1023



void setup() 
{
  Leds.begin();
  Leds.show();

int* AllPins = PlayerJoystickPins[0];
  for ( int i=0;  i<sizeofarray(PlayerJoystickPins);  i++ )
    pinMode(AllPins[i], INPUT);


  for ( int p=0;  p<PAD_COUNT;  p++)
  {
    auto& Pad = Joystick[p];
    Pad.setXAxisRange( AXIS_MIN, AXIS_MAX );
    Pad.setYAxisRange( AXIS_MIN, AXIS_MAX );
    Pad.begin(false);
    Pad.setYAxis( AXIS_MID );
  }
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

void loop() 
{
  for ( int p=0;  p<PLAYER_COUNT; p++ )
  {
    auto PinLeft = PlayerJoystickPins[p][0];
    auto PinRight = PlayerJoystickPins[p][1];
    auto LeftDown = digitalRead( PinLeft ) == HIGH;
    auto RightDown = digitalRead( PinRight ) == HIGH;
    PlayerDirection[p] = LeftDown ? Direction::Left : RightDown ? Direction::Right : Direction::None;
  }
  
/*  
 for ( int p=0;  p<PLAYER_COUNT; p++ )
  {
    auto PlayerColour = PlayerColours[p];
    
    Leds.setPixelColor( PlayerLeds[p][0], PlayerDirection[p]==Direction::Left ? PlayerColour : ColourOff );
    Leds.setPixelColor( PlayerLeds[p][1], PlayerColour );
    Leds.setPixelColor( PlayerLeds[p][2], PlayerDirection[p]==Direction::Right ? PlayerColour : ColourOff );
  }
  */

  FlashCount++;
  FlashCount %= FlashAlternate;
  
  for ( int p=0;  p<PLAYER_COUNT; p++ )
  {
     auto PlayerColour = PlayerColours[p];
    //auto PinLeft = PlayerJoystickPins[p][0];
    //auto PinRight = PlayerJoystickPins[p][1];
    //auto LeftDown = digitalRead( PinLeft ) == HIGH;
    //auto RightDown = digitalRead( PinRight ) == HIGH;
    auto LeftDown = PlayerDirection[p] == Direction::Left;
    auto RightDown = PlayerDirection[p] == Direction::Right;

    auto Flash = (!TestFlash) || ((FlashCount) <= (FlashAlternate/2));
    
    Leds.setPixelColor( PlayerLeds[p][0], LeftDown ? PlayerColour : ColourOff );
    Leds.setPixelColor( PlayerLeds[p][1], Flash ? PlayerColour : ColourOff );
    Leds.setPixelColor( PlayerLeds[p][2], RightDown ? PlayerColour : ColourOff );
 
  
    auto& Pad = Joystick[GamepadIndex[p]];
    int16_t x = AXIS_MID;
    if ( LeftDown )
      x = AXIS_MAX;
    if ( RightDown )
      x = AXIS_MIN;
    Pad.setXAxis( x );
    Pad.sendState();
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




