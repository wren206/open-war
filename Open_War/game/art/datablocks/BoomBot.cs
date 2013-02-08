//-----------------------------------------------------------------------------
// Torque Game Engine Engine
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

datablock PlayerData(BoomBotData : DefaultPlayerData)
{
   shapeFile = "art/shapes/actors/BoomBot/BoomBot.dts";

   boundingBox = "1.1 1.2 2.5";
   pickupRadius = "1.2";
};

function BoomBotData::onReachDestination(%this, %obj)
{
    if (%obj.decal > -1)
        decalManagerRemoveDecal(%obj.decal);
}