using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeizeCommand.Movement
{
    //This Movement script sets the transform.position variable to create a Movement
    public class TransformMovement : AbstractMovement
    {
        [SerializeField] private float speed;
        
        protected override void Move()
        {
            base.Move();

            transform.position += new Vector3(x, y, 0) * speed * Time.deltaTime;
        }
    }
}