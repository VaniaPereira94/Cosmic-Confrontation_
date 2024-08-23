using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static class Constants
    {
        public static readonly string AIM_KEY = "Fire2";
        public static readonly string SHOOT_KEY = "Fire1";
        public static readonly string SHOOT = "ShootAnim";
        public static readonly string PICK = "Pick";
        public static readonly string LAZER_BULLET_PLAYER = "LazerBulletPlayer";
        public static readonly string LAZER_BULLET_ENEMY = "LazerBulletEnemy";
        public static readonly string MEDICINE = "Medicine";
        public static readonly string TREASURE = "Treasure";
        public static readonly string SPINE_ROTATION = "SpineRotation";
        public static readonly string DEATH_COLLDER = "DeathCollider";
        public static readonly string PLAYER_TAG = "Player";
    }

    public static class Animations
    {
        public static readonly string WALKING = "isWalking";
        public static readonly string DYING = "isDying";
        public static readonly string PICKING = "isPicking";
        public static readonly string SHOOTING = "isShooting";
        public static readonly string JUMPING = "isJumping";
        public static readonly string GRABING = "isGrabing";
        public static readonly string RUNNING = "isRunning";

        public static readonly string HOLE_CLOSING = "HoleClosing";
    }

    public static class Environments
    {
        public static readonly string FOREST = "Forest";
        public static readonly string CAMP = "Camp";
        public static readonly string CAVE = "Cave";
        public static readonly string AFTER_MAZE = "AfterMaze";
        public static readonly string PYRAMID = "Pyramid";

        public static List<string> GetValues()
        {
            return new List<string> {
                FOREST,
                CAMP,
                CAVE
            };
        }
    }

    public static class SceneNames
    {
        public static readonly string BEACH_AND_FOREST = "BeachAndForest";
        public static readonly string CAVE_AND_PYRAMID = "CaveAndPyramid";
    }

    public static void PlayAnimation(Animator animator, string animation)
    {
        animator.SetBool(Animations.WALKING, false);
        animator.SetLayerWeight(animator.GetLayerIndex(Constants.SHOOT), 0f);
        animator.SetBool(animation, true);
    }

    public static void CheckIfIsDead(Collider collision, HealthManager healthManager, string bulletRef, ref bool isDead)
    {
        if (collision.gameObject.CompareTag(bulletRef))
        {
            LaserBulletScript laserScript = collision.GetComponent<LaserBulletScript>();
            healthManager.TakeDamage(laserScript.Damage);
            isDead = healthManager.Health <= 0;
        }

        if (collision.gameObject.CompareTag(Constants.DEATH_COLLDER))
        {
            healthManager.TakeAllDamage();
            isDead = true;
        }
    }

    public static void CheckIfIsDead(Collision collision, HealthManager healthManager, string bulletRef, ref bool isDead)
    {
        if (collision.gameObject.CompareTag(bulletRef))
        {
            LaserBulletScript laserScript = collision.gameObject.GetComponent<LaserBulletScript>();
            healthManager.TakeDamage(laserScript.Damage);
            isDead = healthManager.Health <= 0;
        }

        if (collision.gameObject.CompareTag(Constants.DEATH_COLLDER))
        {
            healthManager.TakeAllDamage();
            isDead = true;
        }
    }

    public static float GetDistanceBetween2Objects(GameObject object1, GameObject object2)
    {
        return Vector3.Distance(object1.transform.position, object2.transform.position);
    }
}