//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Misc server commands avialable to clients
//-----------------------------------------------------------------------------

function serverCmdSuicide(%client)
{
   if (isObject(%client.player))
      %client.player.kill("Suicide");
}

function serverCmdPlayCel(%client,%anim)
{
   if (isObject(%client.player))
      %client.player.playCelAnimation(%anim);
}

function serverCmdTestAnimation(%client, %anim)
{
   if (isObject(%client.player))
      %client.player.playTestAnimation(%anim);
}

function serverCmdPlayDeath(%client)
{
   if (isObject(%client.player))
      %client.player.playDeathAnimation();
}

// ----------------------------------------------------------------------------
// Throw/Toss
// ----------------------------------------------------------------------------

function serverCmdThrow(%client, %data)
{
   %player = %client.player;
   if(!isObject(%player) || %player.getState() $= "Dead" || !$Game::Running)
      return;
   switch$ (%data)
   {
      case "Weapon":
         %item = (%player.getMountedImage($WeaponSlot) == 0) ? "" : %player.getMountedImage($WeaponSlot).item;
         if (%item !$="")
            %player.throw(%item);
      case "Ammo":
         %weapon = (%player.getMountedImage($WeaponSlot) == 0) ? "" : %player.getMountedImage($WeaponSlot);
         if (%weapon !$= "")
         {
            if(%weapon.ammo !$= "")
               %player.throw(%weapon.ammo);
         }
      default:
         if(%player.hasInventory(%data.getName()))
            %player.throw(%data);
   }
}

// ----------------------------------------------------------------------------
// Force game end and cycle
// Probably don't want this in a final game without some checks.  Anyone could
// restart a game.
// ----------------------------------------------------------------------------

function serverCmdFinishGame()
{
   cycleGame();
}

// ----------------------------------------------------------------------------
// Cycle weapons
// ----------------------------------------------------------------------------

function serverCmdCycleWeapon(%client, %direction)
{
   %client.getControlObject().cycleWeapon(%direction);
}

// ----------------------------------------------------------------------------
// Unmount current weapon
// ----------------------------------------------------------------------------

function serverCmdUnmountWeapon(%client)
{
   %client.getControlObject().unmountImage($WeaponSlot);
}

// ----------------------------------------------------------------------------
// Weapon reloading
// ----------------------------------------------------------------------------

function serverCmdReloadWeapon(%client)
{
   %player = %client.getControlObject();
   %image = %player.getMountedImage($WeaponSlot);
   
   // Don't reload if the weapon's full.
   if (%player.getInventory(%image.ammo) == %image.ammo.maxInventory)
      return;
      
   if (%image > 0)
      %image.clearAmmoClip(%player, $WeaponSlot);
}

// ----------------------------------------------------------------------------
// Camera commands
// ----------------------------------------------------------------------------

function serverCmdorbitCam(%client)
{
   %client.camera.setOrbitObject(%client.player, mDegToRad(20) @ "0 0", 0, 5.5, 5.5);
   %client.camera.camDist = 5.5;
   %client.camera.controlMode = "OrbitObject";
}
function serverCmdoverheadCam(%client)
{
   %client.camera.position = VectorAdd(%client.player.position, "20 0 30");
   %client.camera.lookAt(%client.player.position);
   %client.camera.controlMode = "Overhead"; 
}

function serverCmdtoggleCamMode(%client)
{
   if(%client.camera.controlMode $= "Overhead")
   {
      serverCmdorbitCam(%client);
   }
   else if(%client.camera.controlMode $= "OrbitObject")
   {
      serverCmdoverheadCam(%client);
   }
}

function serverCmdadjustCamera(%client, %adjustment)
{
   if(%client.camera.controlMode $= "OrbitObject")
   {
      if(%adjustment == 1)
         %n = %client.camera.camDist + 0.5;
      else
         %n = %client.camera.camDist - 0.5;
      
      if(%n < 0.5)
         %n = 0.5;
         
      if(%n > 15)
         %n = 15.0;
         
      %client.camera.setOrbitObject(%client.player, %client.camera.getRotation(), 0, %n, %n);
      %client.camera.camDist = %n;
   }
   if(%client.camera.controlMode $= "Overhead")
   {
      %client.camera.position = VectorAdd(%client.camera.position, "20 0 " @ %adjustment);
   }
}

function createBuilding(%client, %pos, %start, %ray, %shapeName, %class, %collisionType, %scale)
{
    // find end of search vector
    %ray = VectorScale(%ray, 2000);
    %end = VectorAdd(%start, %ray);
    
    // set up to look for the terrain
    %searchMasks = $TypeMasks::TerrainObjectType;

    // search!
    %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    // If the terrain object was found in the scan
    if( %scanTarg )
    {
        // Note:  getWord(%scanTarg, 0) will get the SimObject id of the object 
        // that the button click intersected with.  This is useful if you don't 
        // want to place buildings on certain other objects.  For instance, you 
        // could include TSStatic objects in your search masks and check to see 
        // what you clicked on - then don't place if it's another building.
        %obj = getWord(%scanTarg, 0);
        // get the world position of the click
        %pos = getWords(%scanTarg, 1, 3);

        // spawn a new object at the intersection point
        %obj = new TSStatic()
        {
            position = %pos;
            shapeName = %shapeName;
            class = %class;
            collisionType = %collisionType;
            scale = %scale;
        };

        // Add the new object to the MissionCleanup group
        MissionCleanup.add(%obj);
        
        // Set up a spawn point for new troops to arrive at.
        if (!isObject(Team1SpawnGroup))
        {
            new SimGroup(Team1SpawnGroup)
            {
                canSave = "1";
                canSaveDynamicFields = "1";
                    enabled = "1";
            };

            MissionGroup.add(Team1SpawnGroup);
        }
        
        %spawnName = "team1Spawn" @ %obj.getId();
        %point = new SpawnSphere(%spawnName)
        {
            radius = "1";
            dataBlock      = "SpawnSphereMarker";
            spawnClass     = $Game::DefaultPlayerClass;
            spawnDatablock = $Game::DefaultPlayerDataBlock;
        };
        %point.position = VectorAdd(%obj.getPosition(), "0 5 2");
        Team1SpawnGroup.add(%point);
        MissionCleanup.add(%point);
    }
}

function serverCmdcreateBuilding_orcburrow(%client, %pos, %start, %ray)
{
    createBuilding(%client, %pos, %start, %ray, "art/shapes/orcburrow/orcburrow.dts", "barracks", "Visible Mesh", "0.5 0.5 0.5");
}

function serverCmdcreateBuilding_commandtent(%client, %pos, %start, %ray)
{
    createBuilding(%client, %pos, %start, %ray, "art/shapes/buildings/MEDIUM-DETAILED/command-tent/commandtentlvl0.cached.dts", "commandtent", "Visible Mesh", "0.5 0.5 0.5");
}

function serverCmdmovePlayer(%client, %pos, %start, %ray)
{
    //echo(" -- " @ %client @ ":" @ %client.player @ " moving");

    // Get access to the AI player we control
    %ai = findTeam1Leader();

    %ray = VectorScale(%ray, 1000);
    %end = VectorAdd(%start, %ray);

    // only care about terrain objects
    %searchMasks = $TypeMasks::TerrainObjectType | $TypeMasks::StaticTSObjectType | 
    $TypeMasks::InteriorObjectType | $TypeMasks::ShapeBaseObjectType | $TypeMasks::StaticObjectType;

    // search!
    %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    // If the terrain object was found in the scan
    if( %scanTarg )
    {
        %pos = getWords(%scanTarg, 1, 3);
        // Get the normal of the location we clicked on
        %norm = getWords(%scanTarg, 4, 6);

        // Set the destination for the AI player to
        // make him move
        if (isObject(Team1List))
        {
            %c = 0;
            %end = Team1List.getCount();
            %unit = Team1List.getObject(0);
            while (isObject(%unit))
            {
                if (%unit.isSelected)
                {
                    %dest = VectorSub(%pos, %unit.destOffset);
                    %unit.setMoveDestination( %dest );
                }
                %c++;
                if (%c < %end)
                    %unit = Team1List.getObject(%c);
                else
                    %unit = 0;
            }
        }
        else
            %ai.setMoveDestination( %pos );
    }
}

function serverCmdcheckTarget(%client, %pos, %start, %ray)
{
   %player = %client.player;
   
   %ray = VectorScale(%ray, 1000);
   %end = VectorAdd(%start, %ray);

   // Only care about players this time
   %searchMasks = $TypeMasks::PlayerObjectType | $TypeMasks::StaticTSObjectType
         | $TypeMasks::StaticObjectType;

   // Search!
   %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    // If an enemy AI object was found in the scan
    if( %scanTarg )
    {
        // Get the enemy ID
        %target = firstWord(%scanTarg);
        %targetClass = %target.getClassName();
        if (%target.class $= "barracks")
        {
            serverCmdspawnTeammate(%client, %target);
        }
        else if (%targetClass $= "AIPlayer")
        {
            if (%target.team != 1)
            {
                // Cause our AI object to aim at the target
                // offset (0, 0, 1) so you don't aim at the target's feet

                if (isObject(Team1List))
                {
                    %c = 0;
                    %unit = Team1List.getObject(0);
                    while (isObject(%unit))
                    {
                        if (%unit.isSelected)
                        {
                            %unit.mountImage(Lurker, 0);
                            %targetData = %target.getDataBlock();
                            %z = getWord(%targetData.boundingBox, 2) * 2;
                            %offset = "0 0" SPC %z;
                            %unit.setAimObject(%target, %offset);

                            // Tell our AI object to fire its weapon
                            %unit.setImageTrigger(0, 1);
                        }
                        %c++;
                        %unit = Team1List.getObject(%c);
                    }
                }
            }
            else
            {
                if ($SelectToggled)
                {
                    multiSelect(%target);
                }
                else
                {
                    cleanupSelectGroup();
                    %target.isSelected = true;
                    %target.isLeader = true;
                }
            }
        }
        else
        {
            serverCmdstopAttack(%client);
            if (!$SelectToggled)
                cleanupSelectGroup();
        }
    }
    else
    {
        serverCmdstopAttack(%client);
        if (!$SelectToggled)
            cleanupSelectGroup();
    }
}

function serverCmdstopAttack(%client)
{
    // If no valid target was found, or left mouse
    // clicked again on terrain, stop firing and aiming
    for (%c = 0; %c < Team1List.getCount(); %c++)
    {
        %unit = Team1List.getObject(%c);
        %unit.setAimObject(0);
        %unit.schedule(150, "setImageTrigger", 0, 0);
    }
}

function serverCmdspawnTeammate(%client, %source)
{
    // Create a new, generic AI Player
    // Position will be at the camera's location
    // Datablock will determine the type of actor
    %spawnName = "team1Spawn" @ %source.getId();

    // Defaults
    %spawnClass      = $Game::DefaultPlayerClass;
    %spawnDataBlock  = $Game::DefaultPlayerDataBlock;

    // Overrides by the %spawnPoint
    if (isDefined("%spawnName.spawnClass"))
    {
     %spawnClass = %spawnName.spawnClass;
     %spawnDataBlock = %spawnName.spawnDatablock;
    }
    else if (isDefined("%spawnName.spawnDatablock"))
    {
     // This may seem redundant given the above but it allows
     // the SpawnSphere to override the datablock without
     // overriding the default player class
     %spawnDataBlock = %spawnName.spawnDatablock;
    }

    %spawnProperties = %spawnName.spawnProperties;
    %spawnScript     = %spawnName.spawnScript;

    // Spawn with the engine's Sim::spawnObject() function
    %newBot = spawnObject(%spawnClass, %spawnDatablock, "",
                        %spawnProperties, %spawnScript);

    %spawnLocation = GameCore::pickPointInSpawnSphere(%newBot, %spawnName);
    %newBot.setTransform(%spawnLocation);
    %newBot.team = 1;

    %newBot.clearWeaponCycle();

    %newBot.setInventory(Lurker, 1);
    %newBot.setInventory(LurkerClip, %newBot.maxInventory(LurkerClip));
    %newBot.setInventory(LurkerAmmo, %newBot.maxInventory(LurkerAmmo));
    %newBot.addToWeaponCycle(Lurker);

    if (%newBot.getDatablock().mainWeapon.image !$= "")
    {
        %newBot.mountImage(%newBot.getDatablock().mainWeapon.image, 0);
    }
    else
    {
        %newBot.mountImage(Lurker, 0);
    }
    
    // This moves our new bot away from the front door a ways to make room for 
    // other bots as we spawn them.
    %x = getRandom(-10, 10);
    %y = getRandom(4, 10);
    %vec = %x SPC %y SPC "0";
    %dest = VectorAdd(%newBot.getPosition(), %vec);
    %newBot.setMoveDestination(%dest);
    
    addTeam1Bot(%newBot);
}

function addTeam1Bot(%bot)
{
    // We'll create a SimSet to track our Team1 bots if it hasn't been created already
    if (!isObject(Team1List))
    {
        new SimSet(Team1List);
        MissionCleanup.add(Team1List);
    }
    
    // And then add our bot to the Team1 list.
    Team1List.add(%bot);
}

function serverCmdtoggleMultiSelect(%client, %flag)
{
    if (%flag)
        $SelectToggled = true;
    else
        $SelectToggled = false;
}

function multiSelect(%target)
{
    if (!isObject(Team1List))
    {
        new SimSet(Team1List);
        MissionCleanup.add(Team1List);
    }
    
    %leader = findTeam1Leader();
    if (isObject(%leader))
    {
        %target.destOffset = VectorSub(%leader.getPosition(), %target.getPosition());
    }
    else
    {
        %target.destOffset = "0 0 0";
        %target.isLeader = true;
    }

    %target.isSelected = true;
}

function findTeam1Leader()
{
    if (!isObject(Team1List))
    {
        new SimSet(Team1List);
        MissionCleanup.add(Team1List);
    }

    for (%c = 0; %c < Team1List.getCount(); %c++)
    {
        %unit = Team1List.getObject(%c);
        if (%unit.isLeader)
            return %unit;
    }

    return 0;
}

function cleanupSelectGroup()
{
    if (!isObject(Team1List))
    {
        new SimSet(Team1List);
        MissionCleanup.add(Team1List);
    }
    
    for (%c = 0; %c < Team1List.getCount(); %c++)
    {
        %temp = Team1List.getObject(%c);
        %temp.isSelected = false;
        %temp.isLeader = false;
        %temp.destOffset = "0 0 0";
    }
}