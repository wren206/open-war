//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// PlayGui is the main TSControl through which the game is viewed.
// The PlayGui also contains the hud controls.
//-----------------------------------------------------------------------------

function PlayGui::onWake(%this)
{
   // Turn off any shell sounds...
   // sfxStop( ... );

   $enableDirectInput = "1";
   activateDirectInput();

   // Message hud dialog
   if ( isObject( MainChatHud ) )
   {
      Canvas.pushDialog( MainChatHud );
      chatHud.attach(HudMessageVector);
   }      
   
   // just update the action map here
   moveMap.push();

   // hack city - these controls are floating around and need to be clamped
   if ( isFunction( "refreshCenterTextCtrl" ) )
      schedule(0, 0, "refreshCenterTextCtrl");
   if ( isFunction( "refreshBottomTextCtrl" ) )
      schedule(0, 0, "refreshBottomTextCtrl");
}

function PlayGui::onSleep(%this)
{
   if ( isObject( MainChatHud ) )
      Canvas.popDialog( MainChatHud );
   
   // pop the keymaps
   moveMap.pop();
}

function PlayGui::clearHud( %this )
{
   Canvas.popDialog( MainChatHud );

   while ( %this.getCount() > 0 )
      %this.getObject( 0 ).delete();
}

//-----------------------------------------------------------------------------

function refreshBottomTextCtrl()
{
   BottomPrintText.position = "0 0";
}

function refreshCenterTextCtrl()
{
   CenterPrintText.position = "0 0";
}

// onMouseDown is called when the left mouse
// button is clicked in the scene
// %pos is the screen (pixel) coordinates of the mouse click
// %start is the world coordinates of the camera
// %ray is a vector through the viewing 
// frustum corresponding to the clicked pixel
function PlayGui::onMouseDown(%this, %pos, %start, %ray)
{
    // If we're in building placement mode ask the server to create a building for
    // us at the point that we clicked.
    if (%this.placingBuilding == true)
    {
        // Request a building at the clicked coordinates from the server.
        commandToServer(%this.building, %pos, %start, %ray);

        // Clear the building placement flag.
        %this.placingBuilding = false;
    }
    else
    {
        // Ask the server to let us attack a target at the clicked position.
        commandToServer('checkTarget', %pos, %start, %ray);
    }
}

// This function is the callback that handles our new button.  When you click it
// the button tells the PlayGui that we're now in building placement mode.
function orcBurrowButton::onClick(%this)
{
    PlayGui.building = 'createBuilding_orcburrow';
    PlayGui.placingBuilding = true;
}

function testButton::onClick(%this)
{
    PlayGui.building = 'createBuilding_commandtent';
    PlayGui.placingBuilding = true;
}

//Camera Right
function GuiMouseRight::onMouseEnter(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvRightAction = $movementSpeed;
}

function GuiMouseRight::onMouseLeave(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvRightAction = 0;
}

//Camera Left
function GuiMouseLeft::onMouseEnter(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvLeftAction = $movementSpeed;
}

function GuiMouseLeft::onMouseLeave(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvLeftAction = 0;
}

//Camera Up
function GuiMouseTop::onMouseEnter(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvForwardAction = $movementSpeed;
}

function GuiMouseTop::onMouseLeave(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvForwardAction = 0;
}

//Camera Down
function GuiMouseBottom::onMouseEnter(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvBackwardAction = $movementSpeed;
}

function GuiMouseBottom::onMouseLeave(%this,%modifier,%mousePoint,%mouseClickCount)
{
    $mvBackwardAction = 0;
}

// onRightMouseDown is called when the right mouse
// button is clicked in the scene
// %pos is the screen (pixel) coordinates of the mouse click
// %start is the world coordinates of the camera
// %ray is a vector through the viewing 
// frustum corresponding to the clicked pixel
function PlayGui::onRightMouseDown(%this, %pos, %start, %ray)
{   
   commandToServer('movePlayer', %pos, %start, %ray);

    %ray = VectorScale(%ray, 1000);
    %end = VectorAdd(%start, %ray);

    // only care about terrain objects
    %searchMasks = $TypeMasks::TerrainObjectType | $TypeMasks::StaticTSObjectType | 
        $TypeMasks::InteriorObjectType | $TypeMasks::ShapeBaseObjectType |
        $TypeMasks::StaticObjectType;

    // search!
    %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    if (%scanTarg)
    {
        // Get access to the AI player we control
        %ai = LocalClientConnection.player;

        // Get the X,Y,Z position of where we clicked
        %pos = getWords(%scanTarg, 1, 3);

        // Get the normal of the location we clicked on
        %norm = getWords(%scanTarg, 4, 6);

        // Create a new decal using the decal manager
        // arguments are (Position, Normal, Rotation, Scale, Datablock, Permanent)
        // AddDecal will return an ID of the new decal, which we will
        // store in the player
        decalManagerAddDecal(%pos, %norm, 0, 1, "ScorchBigDecal", false);
    }
}