using UnityEngine;
/// <summary>
/// This additional utility class is not mandatory but since i use to use
/// coroutine frequently, i needed this to not repeat same function over and 
/// over again. Its not limited to just coroutine, extend this to add more
/// repetetive task. I strongly follow DRY (DONT REPEAT YOURSELF) xD
/// </summary>
public abstract class UtilityFunctions : MonoBehaviour {
    protected Coroutine _currentRunningCoroutine;
    protected void ResetCoroutine() { 
        if(_currentRunningCoroutine != null) { 
            StopCoroutine(_currentRunningCoroutine);
            _currentRunningCoroutine = null;
        }    
    }
}
