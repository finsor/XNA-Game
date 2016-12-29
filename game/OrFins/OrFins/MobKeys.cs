using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace OrFins
{
    struct PathRecord
    {
        public Vector2 destination;
        public bool rightkey;
        public bool leftkey;
        public bool jumpkey;

        public PathRecord(Vector2 destination, bool rightkey, bool leftkey, bool jumpkey)
        {
            this.destination = destination;
            this.rightkey = rightkey;
            this.leftkey = leftkey;
            this.jumpkey = jumpkey;
        }
    }

    class MobKeys : BaseKeys
    {
        private Mob mob;
        private Map map;

        private int indexForRandomState;
        private int randomTime;

        private bool right, left, jump, skill;

        private const float JUMP_MAX_DISTANCE = 101f;

        private Queue<PathRecord> path;
        private List<Platform> platformsThatWereSearched;

        private int time_counter;

        #region Key functions
        public override bool UpKey()
        {
            return false;
        }
        public override bool DownKey()
        {
            return false;
        }
        public override bool RightKey()
        {
            return right;
        }
        public override bool LeftKey()
        {
            return left;
        }
        public override bool JumpKey()
        {
            return jump;
        }
        public override bool AttackKey()
        {
            return false;
        }
        public override bool SkillKey()
        {
            return skill;
        }
        public override bool RopeKey()
        {
            return false;
        }
        public override bool PickupKey()
        {
            return false;
        }
        #endregion

        public MobKeys(Mob mob, Map map)
        {
            this.mob = mob;
            this.map = map;
            path = new Queue<PathRecord>();
            platformsThatWereSearched = new List<Platform>();

            right = (Service.Random(0, 2) > 0) ? true : false;
            left = !right;

            indexForRandomState = 0;
            randomTime = Service.Random(30, 240);

            time_counter = 0;
        }

        public override void Update()
        {
            #region dont move if hit
            if (mob.isHit)
            {
                right = left = jump = false;
                return;
            }
            #endregion

            #region Generate a random movement as long as mob is not attacked
            if (!mob.isAttacked)
            {
                // Stand still once in 20 seconds
                if (Service.Random(0, 1200) == 7)
                {
                    left = false;
                    right = false;
                    jump = false;

                    randomTime = 30;
                    indexForRandomState = 0;
                }
                else if (!mob.canJump)
                {
                    // Change direction if reached end of platform or after a random time
                    if ((right && !mob.downRightSensor.IsOn) ||
                       (left && !mob.downLeftSensor.IsOn) ||
                       (++indexForRandomState == randomTime))
                    {
                        left = !(right = !right);
                        indexForRandomState = 0;
                        randomTime = Service.Random(30, 240);
                    }
                }
                else // If mob can jump
                {
                    // Change direction if reached end of map or after a random time
                    if ((right && map.rightLimit - mob.position.X <= 10) ||
                       (left && mob.position.X - map.leftLimit <= 10) ||
                       (++indexForRandomState == randomTime))
                    {
                        left = !(right = !right);

                        indexForRandomState = 0;
                        randomTime = Service.Random(30, 240);
                    }

                    // Jump if countered an obstacle, or randomally if below platform
                    if ((right && mob.rightSensor.IsOn) ||
                        (left && mob.leftSensor.IsOn) ||
                        (IsBelowPlat(map.mapMask) && ++indexForRandomState == randomTime)) // (below plat && random time)
                    {
                        jump = true;

                        indexForRandomState = 0;
                        randomTime = Service.Random(30, 240);
                    }
                    else
                    {
                        jump = false;
                    }

                }
            }
            #endregion

            #region Once attacked, mob follows player
            else
            {
                Player player = mob.playerToFollow;

                if (mob.CanLaunchSkill && !player.IsDead && Service.Random(0, 50) == 7 && Vector2.Distance(mob.position, player.position) <= Skill.MAX_DISTANCE)
                {
                    skill = true;
                    return;
                }
                else
                {
                    skill = false;
                }

                // If mob and player are close enough, then mob doesnt move
                if (Vector2.Distance(mob.position, player.position) <= mob.speed.X)
                {
                    right = left = jump = false;
                    return;
                }

                if (mob.canJump)
                {
                    if (mob.basePlat == player.basePlat)
                    {
                        path.Clear();
                        left = right = jump = false;
                        GetToPosition(player.position);

                        if (Vector2.Distance(mob.position, player.position) < 3)
                        {
                            mob.position = player.position;
                        }
                        return;
                    }

                    // Once in a second, regenerate path
                    if (++time_counter % 60 == 0)
                    {
                        time_counter = 0;
                        FillPath(map.platforms, player);
                    }

                    // If path is not empty, then follow it
                    if (path.Count > 0)
                    {
                        if (Vector2.Distance(mob.position, path.Peek().destination) > 3)
                        {
                            left = right = jump = false;
                            GetToPosition(path.Peek().destination);
                        }
                        else
                        {
                            PathRecord record = path.Dequeue();
                            mob.position = record.destination;
                            right = record.rightkey;
                            left = record.leftkey;
                            jump = record.jumpkey;
                        }
                    }
                }

                // If not mob.canJump
                else
                {
                    if (player.position.X < mob.position.X)
                    {
                        left = true;
                    }
                    else // move right
                    {
                        left = false;
                    }
                    right = !left;

                    // If mob blocked
                    if (mob.leftSensor.IsOn ||
                        (left && !mob.downLeftSensor.IsOn))
                    {
                        left = false;
                    }

                    if (mob.rightSensor.IsOn ||
                        (right && !mob.downRightSensor.IsOn))
                    {
                        right = false;
                    }
                }
            }
            #endregion
        }

        #region AI Functions
        // Get whether mob is below another platform or not.
        private bool IsBelowPlat(MapRecord[,] mapMask)
        {
            int search_start = (int)(mob.position.Y - 50);
            int jump_height = (int)(mob.position.Y - 101);
            int mobX = (int)mob.position.X;

            if (jump_height < 1)
                return false;

            for (int i = search_start; i >= jump_height; i--)
            {
                if (mapMask[mobX, i].isLand && !mapMask[mobX, i - 1].isLand)
                    return true;
            }

            return false;
        }

        // Moves mob to a requested position.
        private void GetToPosition(Vector2 destination)
        {
            if (destination.X > mob.position.X)
            {
                right = true;
            }
            if (destination.X < mob.position.X)
            {
                left = true;
            }
            if ((mob.leftSensor.IsOn && left) ||
                (mob.rightSensor.IsOn && right))
            {
                jump = true;
            }
            else
            {
                jump = false;
            }
        }

        // Function used to fill path in order for mob to reach playerToFollow.
        // Depends on whether playerToFollow is above or below mob.
        private void FillPath(List<Platform> platforms, Player player)
        {
            path.Clear();
            platformsThatWereSearched.Clear();

            // If player is above bot
            if (player.basePlat.position.Y < mob.basePlat.position.Y)
            {
                UP_RECURSION(player.basePlat, player);
                path.Enqueue(new PathRecord(player.position, false, false, false));
            }
            // If player is below bot (same height taken care of in Update)
            else
            {
                DOWN(mob.basePlat);
            }
        }

        #region Functions used to fill path for mob ascenting

        // Main function used to fill the uprise path
        private bool UP_RECURSION(Platform plat, Player player)
        {
            // If mob's plat was reached, then a path was found.
            if (plat == mob.basePlat)
                return true;

            // If platform was already searched, then escape.
            if (platformsThatWereSearched.Contains(plat))
            {
                return false;
            }

            // Mark platform as already searched.
            platformsThatWereSearched.Add(plat);

            MapRecord[,] mapMask = map.mapMask;
            Vector2 searchedPos;
            PathRecord thisPathRecord;

            bool wasFound;

            #region If mob is right to platform, search right side first
            if (mob.position.X > plat.rightmostPart.X)
            {
                thisPathRecord = UPSearchRightOrLeft(plat, out wasFound, plat.rightmostPart, +0.1f);
                searchedPos = thisPathRecord.destination;

                if (wasFound && UP_RECURSION(map.mapMask.At(searchedPos).platform, player))
                {
                    path.Enqueue(thisPathRecord);
                    return true;
                }
            }
            #endregion

            #region If mob is left to platform, search left side first
            if (mob.position.X < plat.leftmostPart.X)
            {
                if (mob.position.X < plat.leftmostPart.X)
                {
                    thisPathRecord = UPSearchRightOrLeft(plat, out wasFound, plat.leftmostPart, -0.1f);
                    searchedPos = thisPathRecord.destination;

                    if (wasFound && UP_RECURSION(mapMask.At(searchedPos).platform, player))
                    {
                        path.Enqueue(thisPathRecord);
                        return true;
                    }
                }
            }
            #endregion

            // If both failed, then mob is below plat.

            #region Search middle first
            thisPathRecord = SearchMiddle(plat, out wasFound);
            searchedPos = thisPathRecord.destination;

            if (wasFound && UP_RECURSION(mapMask.At(searchedPos).platform, player))
            {
                path.Enqueue(thisPathRecord);
                return true;
            }
            #endregion

            #region Once middle search failed, search left and right, starting with the side that's closer to the mob
            Vector2 first_searched_part;
            Vector2 other_part;
            // If mob is right to plat.middlePart, then search right first
            if (mob.position.X > plat.middlePart.X)
            {
                first_searched_part = plat.rightmostPart;
                other_part = plat.leftmostPart;

                thisPathRecord = UPSearchRightOrLeft(plat, out wasFound, plat.rightmostPart, +0.1f);
                searchedPos = thisPathRecord.destination;

                if (wasFound && UP_RECURSION(map.mapMask.At(searchedPos).platform, player))
                {
                    path.Enqueue(thisPathRecord);
                    return true;
                }
            }
            else // Search left first
            {
                first_searched_part = plat.leftmostPart;
                other_part = plat.rightmostPart;

                thisPathRecord = UPSearchRightOrLeft(plat, out wasFound, plat.leftmostPart, -0.1f);
                searchedPos = thisPathRecord.destination;

                if (wasFound && UP_RECURSION(mapMask.At(searchedPos).platform, player))
                {
                    path.Enqueue(thisPathRecord);
                    return true;
                }
            }

            // Search the other side
            if (first_searched_part == plat.rightmostPart)
            {
                // Search Left
                thisPathRecord = UPSearchRightOrLeft(plat, out wasFound, plat.leftmostPart, -0.1f);
                searchedPos = thisPathRecord.destination;

                if (wasFound && UP_RECURSION(mapMask.At(searchedPos).platform, player))
                {
                    path.Enqueue(thisPathRecord);
                    return true;
                }
            }
            else
            {
                // Search right
                thisPathRecord = UPSearchRightOrLeft(plat, out wasFound, plat.rightmostPart, +0.1f);
                searchedPos = thisPathRecord.destination;

                if (wasFound && UP_RECURSION(map.mapMask.At(searchedPos).platform, player))
                {
                    path.Enqueue(thisPathRecord);
                    return true;
                }
            }


            #endregion

            // If all failed then there is no possible path
            return (false);
        }

        // Function used to search a path to a current_plat from left or right
        private PathRecord UPSearchRightOrLeft(Platform current_plat, out bool wasFound, Vector2 platPart, float rotation_addition)
        {
            Vector2 centerOfCircle = platPart;
            float rotation = 0;
            Vector2 searchedPos = Vector2.Zero;
            Matrix rotationMat;
            Vector2 jumpLength = Vector2.UnitY * JUMP_MAX_DISTANCE;
            Vector2 normalizedVec;
            MapRecord record;
            MapRecord[,] mapMask = map.mapMask;

            left = right = wasFound = false;

            // Look around the pit of the platform
            while (Math.Abs(rotation) <= MathHelper.TwoPi)
            {
                Matrix.CreateRotationZ(rotation, out rotationMat);
                searchedPos = centerOfCircle + Vector2.Transform(jumpLength, rotationMat);

                // If out of map, continue.
                if (searchedPos.X < map.leftLimit || searchedPos.Y > map.bottomLimit ||
                    searchedPos.X > map.rightLimit || searchedPos.Y < map.topLimit)
                {
                    rotation += rotation_addition;
                    continue;
                }

                // If hit another platform
                record = map.mapMask.At(searchedPos);
                if (record.isLand &&
                    record.platform != current_plat &&
                    record.platform.position.Y > current_plat.position.Y)
                {

                    // Find the shortest part of the vector that hit hit
                    normalizedVec = Vector2.Normalize(Vector2.Transform(jumpLength, rotationMat));

                    // Sub the normalized vector from the searchPos while background wasnt hit 
                    while (map.mapMask.At(searchedPos - normalizedVec).isLand)
                    {
                        searchedPos -= normalizedVec;
                    }

                    // If searchedPos is right to centerOfCircle then mob has to move left
                    if (searchedPos.X > centerOfCircle.X)
                    {
                        left = true;
                    }

                    // If mob moves left then it doesnt move right and vise versa
                    right = !left;


                    wasFound = true;
                    return (new PathRecord(searchedPos, right, left, true));
                }

                rotation += rotation_addition;
            }

            // If search failed
            wasFound = false;
            return (new PathRecord());
        }

        // Function used to search a path to a current_plat from its middle
        private PathRecord SearchMiddle(Platform current_plat, out bool wasFound)
        {
            MapRecord[,] mapMask = map.mapMask;
            MapRecord record;

            Vector2 centerOfCircle = current_plat.middlePart;
            Vector2 searchedPos = centerOfCircle;

            left = right = wasFound = false;

            // While scanned range is shorter than a jump's length
            while ((searchedPos - centerOfCircle).Y < JUMP_MAX_DISTANCE)
            {
                searchedPos += Vector2.UnitY;

                // If out of map, escape.
                if (searchedPos.Y >= map.bottomLimit)
                {
                    wasFound = false;
                    return (new PathRecord());
                }

                // If hit another platform
                record = map.mapMask.At(searchedPos);
                if (record.isLand &&
                    record.platform != current_plat &&
                    record.platform.position.Y > current_plat.position.Y)
                {
                    // If searchedPos is right to centerOfCircle then mob has to move left
                    if (searchedPos.X > centerOfCircle.X)
                    {
                        left = true;
                    }

                    // If mob moves left then it doesnt move right and vise versa
                    right = !left;

                    wasFound = true;
                    return (new PathRecord(searchedPos, right, left, true));
                }
            }

            // If search failed
            wasFound = false;
            return (new PathRecord());
        }
        #endregion

        #region Functions used to fill path for mob descending

        // Main function used to fill the downfall path
        private void DOWN(Platform plat)
        {
            // If mob is closer to the right tip of the platform
            if (mob.position.X > plat.middlePart.X)
            {
                // Search right. If failed, search left.
                if (!DOWNSearchRightOrLeft(plat.rightmostPart, MathHelper.ToRadians(155)))
                {
                    DOWNSearchRightOrLeft(plat.leftmostPart, MathHelper.ToRadians(-155));
                }
            }
            else
            {
                // Search left. If failed, search right.
                if (!DOWNSearchRightOrLeft(plat.leftmostPart, MathHelper.ToRadians(-155)))
                {
                    DOWNSearchRightOrLeft(plat.rightmostPart, MathHelper.ToRadians(155));
                }
            }
        }

        // Function used to fall down from platform from left or right
        private bool DOWNSearchRightOrLeft(Vector2 plat_requested_part, float rotation)
        {
            MapRecord[,] mapMask = map.mapMask;
            Vector2 normalizedVec;
            Matrix rotationMat;
            Vector2 searchedPos;

            // Create the rotated and normalized vector
            Matrix.CreateRotationZ(rotation, out rotationMat);
            normalizedVec = Vector2.Transform(-Vector2.UnitY, rotationMat);
            normalizedVec.Normalize();

            // While not searching outside of map's limits
            searchedPos = plat_requested_part + 5 * normalizedVec;
            while (searchedPos.X < map.rightLimit &&
                   searchedPos.X > map.leftLimit &&
                   searchedPos.Y < map.bottomLimit)
            {
                if (mapMask.At(searchedPos).isLand)
                {
                    path.Enqueue(new PathRecord(searchedPos, false, false, false));
                    return (true);
                }

                searchedPos += normalizedVec;
            }

            return (false);
        }

        #endregion
        #endregion
    }
}
