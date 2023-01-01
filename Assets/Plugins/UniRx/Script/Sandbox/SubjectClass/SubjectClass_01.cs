using System;
using UniRx;
using UnityEngine;

namespace Sandbox.SubjectClass
{
    public class SubjectClass_01 : MonoBehaviour
    {
        [ContextMenu(nameof(Subject))]
        void Subject()
        {
            // OnError後は､一切値が流れないことを確認
            {
                var source = new Subject<int>();

                source.Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));

                UnityEngine.Debug.Log("OnError");
                source.OnError(new Exception("Exception"));

                source.OnNext(0);
            }
            
            // OnCompleted後は､一切値が流れないことを確認
            {
                var source = new Subject<int>();
                
                source.Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
            
                UnityEngine.Debug.Log("OnCompleted");
                source.OnCompleted();
                
                source.OnNext(0);
            }
        }
        
        
        [ContextMenu(nameof(BehaviourSubject))]
        void BehaviourSubject()
        {
            // BehaviourSubjectは初期値付きのSubject
            var source = new BehaviorSubject<int>(0);

            source.Subscribe(
                i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                () => UnityEngine.Debug.Log($"1## OnCompleted()"));

            source.OnNext(1);

            // BehaviourSubjectのもう一つの特徴は､購読後に最後の値を流すということ
            // (というか､初期値付きというより最後の値をすぐ流すのがBehaviourSubjectの特徴)
            // なので､↓の2回目の購読は､0は流れず1から値が流れる
            source.Subscribe(
                i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                () => UnityEngine.Debug.Log($"2## OnCompleted()"));

            source.OnNext(2);

            source.OnCompleted();

            // OnCompletedした後は､OnNextで値を流したり､購読しても値が流れない
            source.OnNext(3);
            source.Subscribe(
                i => UnityEngine.Debug.Log($"3## OnNext({i})"),
                () => UnityEngine.Debug.Log($"3## OnCompleted()"));
        }

        [ContextMenu(nameof(AsyncSubject))]
        void AsyncSubject()
        {
            // AsyncSubjectは､OnCompletedが発行されるまで､値を流さない.
            // OnCompletedが発行されると､最後の値だけ流すSubject.
            var source = new AsyncSubject<int>();
            
            source.Subscribe(
                i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                () => UnityEngine.Debug.Log($"1## OnCompleted()"));

            for (int i = 0; i < 3; i++)
            {
                source.OnNext(i);
            }
            // ↓OnCompletedするまで､値が流れないことを確認する
            UnityEngine.Debug.Log($"OnCompleted");
            source.OnCompleted();

            // OnCompletedの後の購読も､最後の値が流れる
            source.Subscribe(
                i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                () => UnityEngine.Debug.Log($"2## OnCompleted()"));
        }
        
        [ContextMenu(nameof(ReplaySubject))]
        void ReplaySubject()
        {
            // ReplaySubjectは､全ての値をキャッシュしておいて､
            // 購読時に流してくれるSubject.
            var source = new ReplaySubject<int>();

            source.OnNext(0);
            source.OnNext(1);
            
            // 購読した際､↑の発行済みの値も全て流してくれる
            source.Subscribe(
                i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                () => UnityEngine.Debug.Log($"1## OnCompleted()"));
            
            source.OnNext(2);

            source.OnCompleted();

            // OnCompleted後の購読後でも､全ての値を流してくれる特性を持つ
            source.Subscribe(
                i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                () => UnityEngine.Debug.Log($"2## OnCompleted()"));
        }
    }
}