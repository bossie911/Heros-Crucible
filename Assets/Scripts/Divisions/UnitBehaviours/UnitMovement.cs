﻿using DG.Tweening;
using GameStudio.HunterGatherer.Networking;
using GameStudio.HunterGatherer.Networking.Events;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace GameStudio.HunterGatherer.Divisions.UnitBehaviours
{
    /// <summary>Unit behaviour that handles unit movement</summary>
    public class UnitMovement : UnitBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private float arriveDistance = 1f;

        [SerializeField]
        private UnitState activeState = UnitState.Move;

        [SerializeField]
        private UnitState stateIfArrived = UnitState.Idle;

        [SerializeField, Tooltip("How long to do over tweening the units rotation when it arrives, regardless of current rotation")]
        private float arrivalRotationDurationFlat = 0.5f;

        [SerializeField, Tooltip("How long to do over tweening the units rotation when it arrives, scaled with difference in rotation, on top of flat rotation")]
        private float arrivalRotationDurationScaled = 1f;

        [Header("References")]
        [SerializeField]
        public NavMeshAgent navMeshAgent = null;

        private float moveStartTime = 0f;

        private Coroutine moveRoutine;
        private Tween tween;

        public Division division;
        private DivisionGoal goalLastFrame = DivisionGoal.Idle;

        private bool ShouldRun
        {
            get
            {
                return Unit.MoveTarget.HasTargetTransform && Vector3.Distance(Unit.transform.position, Unit.MoveTarget.Position) < Unit.Division.TypeData.DistanceFromEnemiesToStartRunning;
            }
        }

        private bool Arrived
        {
            get
            {
                if (Unit.MoveTarget == null)
                {
                    //Debug.Log("target is null");
                    return true;
                }
                return Vector3.Distance(Unit.transform.position, Unit.MoveTarget.Position) < arriveDistance; 
            }
        }

        /// <summary>This behaviour is active during the move state</summary>
        protected override bool ShouldBeActiveDuringState(UnitState state)
        {
            return activeState == state;
        }

        /// <summary>Start the move routine</summary>
        protected override void StartBehaviour()
        {
            moveRoutine = StartCoroutine(Move());
        }

        /// <summary>Stop the move routine</summary>
        protected override void StopBehaviour()
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }
        }

        /// <summary>Move to the move target until we have arrived</summary>
        private IEnumerator Move()
        {
	        //if (!NetworkServer.active) yield break;
	        // Always set destination once before entering the while loop, so it's not possible to be considered 'arrived' without ever setting the destination
	        if (navMeshAgent.isOnNavMesh && Unit.MoveTarget != null)
	        {
		        navMeshAgent.SetDestination(Unit.MoveTarget.Position);
	        }


	        while (!Arrived)
	        {
		        // Prevents errors from being thrown when the navMeshAgent is not on the ground or disabled.
		        if (navMeshAgent.isOnNavMesh && Unit.MoveTarget != null)
                {
                    navMeshAgent.SetDestination(Unit.MoveTarget.Position);
                    goalLastFrame = division.Goal;
                }

		        yield return null;
	        }

	        // Rotate towards Movetarget direction
	        if (Unit.MoveTarget != null)
	        {
		        float tweenLength = arrivalRotationDurationFlat + arrivalRotationDurationScaled * Vector3.Angle(Unit.transform.rotation * Vector3.forward, Unit.MoveTarget.Direction) / 360f;
		        if (tween != null)
		        {
			        tween.Kill();
		        }
		        tween = Unit.transform.DORotateQuaternion(Quaternion.LookRotation(Unit.MoveTarget.Direction), tweenLength);
	        }

	        // Change state if arrived
	        Unit.State = stateIfArrived;
        }
    }
}