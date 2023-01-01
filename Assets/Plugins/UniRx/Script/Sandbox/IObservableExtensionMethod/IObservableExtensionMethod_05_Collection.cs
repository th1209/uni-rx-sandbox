using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    // コレクションに関する拡張メソッド群.
    // いずれも､OnCompletedが発行されるまで値を貯めておいて､OnCompleted時点で変換した値を返すのが特徴
    // 以下はUniRxでは存在しない模様(Aggregateで大体何とかできるので､そんなに気にしなくて良い)
    //   ToDictionary,ToLookup
    //   Min,Max,MinBy,MaxBy,Average
    //   Count,LongCount
    //   Any,All
    public class IObservableExtensionMethod_05_Collection : MonoBehaviour
    {
        [ContextMenu(nameof(ToArray))]
        void ToArray()
        {
            // ToArrayは､OnCompletedが発行されるまで､OnNextで発行された要素を集めて､配列として変換するメソッド.
            var subject = new Subject<int>();
            subject.ToArray().Subscribe(array =>
            {
                for (int i = 0; i < array.Length; i++)
                {
                    UnityEngine.Debug.Log($"Array value:{array[i]}");
                }
            });
            
            subject.OnNext(1);
            subject.OnNext(2);
            subject.OnNext(3);
            subject.OnCompleted();
        }
        
        [ContextMenu(nameof(ToList))]
        void ToList()
        {
            // ToArrayとほぼ同じ
            var subject = new Subject<int>();
            subject.ToList().Subscribe(list =>
            {
                for (int i = 0; i < list.Count; i++)
                {
                    UnityEngine.Debug.Log($"List value:{list[i]}");
                }
            });
            
            subject.OnNext(1);
            subject.OnNext(2);
            subject.OnNext(3);
            subject.OnCompleted();
        }

        [ContextMenu(nameof(Aggregate))]
        void Aggregate()
        {
            // Aggregateは名前の通りで､集計を行う
            // OnCompleteが発行されるまで､OnNextの各要素を集める
            // ↓ の感じで､第一引数にそれまでの値の集計値､第二引数に今回の値を取る. returnで返した値が次回の第一引数となる.
            // 極めて汎用性が高く､UniRxで他のCollection系メソッドが実装されていないのは､基本これ使ってということなのだろう.
            var subject = new Subject<int>();
            subject.Aggregate((x,y) => x >= y ? x : y ).Subscribe(i =>
            {
                UnityEngine.Debug.Log($"Max value:{i}");
            });
            subject.Aggregate((x,y) => x < y ? x : y ).Subscribe(i =>
            {
                UnityEngine.Debug.Log($"Min value:{i}");
            });

            // 注目すべきは↓の結果. 最初の1回目では値が呼ばれない点に注意...!
            subject.Aggregate((x, y) =>
            {
                UnityEngine.Debug.Log($"Aggregate x:{x} y:{y}");
                return x;
            }).Subscribe(_ => { });

            subject.OnNext(175);
            subject.OnNext(28);
            subject.OnNext(92);
            subject.OnCompleted();
        }

        [ContextMenu(nameof(Aggregate_InitialValue))]
        void Aggregate_InitialValue()
        {
            // ↑で見た感じで､Aggregateは最初の1回は実行されない. なので､初期値を与えるオーバーロードがある.
            // 最初の1回が飛ばされるのは意図しない結果を招きそうなので､基本的にはこっちの利用を推奨か.
            var subject = new Subject<int>();
            subject.Aggregate(0, (x,y) => x < y ? x : y ).Subscribe(i =>
            {
                UnityEngine.Debug.Log($"Min value:{i}");
            });

            subject.Aggregate(new List<int>(), (list, value) =>
            {
                list.Add(value);
                return list;
            }).Subscribe(i =>
            {
                UnityEngine.Debug.Log($"Min value:{i}");
            });

            subject.OnNext(175);
            subject.OnNext(28);
            subject.OnNext(92);
            subject.OnCompleted();
        }
        
        [ContextMenu(nameof(Aggregate_Collect_Items))]
        void Aggregate_Collect_Items()
        {
            // Aggregateのシグネチャは↓の感じで､実はTAccumulateとTSourceの2つの型パラメータを取っている
            // IObservable<TAccumulate> Aggregate(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
            // なので､例えば↓の感じで､コレクションに要素を足して集計､みたいなこともできる!
            var subject = new Subject<int>();
            subject.Aggregate(new List<int>(), (list, v) =>
            {
                list.Add(v);
                return list;
            }).Subscribe(list =>
            {
                UnityEngine.Debug.Log($"List values:{string.Join(',', list)}");
            });

            subject.OnNext(175);
            subject.OnNext(28);
            subject.OnNext(92);
            subject.OnCompleted();
        }

        [ContextMenu(nameof(GroupBy))]
        void GroupBy()
        {
            // GroupByは､名前の通りで､指定した条件でグループを行うメソッド.
            // IGroupedObservableという形で変換が行われる. 実行時の動きが厄介なので､以下参照.
            var subject = new Subject<int>();
            subject.GroupBy(i  => i % 10).Subscribe(groupedObservable =>
            {
                // ここのログは､新しいGroupの登録毎に1回だけ呼ばれる
                UnityEngine.Debug.Log($"Grouping start. groupKey:{groupedObservable.Key}");
                // 以降の購読処理は､毎OnNext毎に呼ばれるのだが､ここではToArrayで集計しているため､OnCompletedが発行されるまで待つ挙動になる
                groupedObservable
                    .ToArray()
                    .Select(array => string.Join(",", array))
                    .Subscribe(
                        str => UnityEngine.Debug.Log($"GroupBy OnNext groupedValues:{str}"),
                        ex => UnityEngine.Debug.Log($"GroupBy OnException({ex})"),
                        () => UnityEngine.Debug.Log($"GroupBy OnCompleted()")
                    );
            });

            subject.OnNext(13);
            subject.OnNext(1);
            subject.OnNext(11);
            subject.OnNext(42);
            subject.OnNext(21);
            subject.OnNext(12);
            subject.OnNext(23);
            UnityEngine.Debug.Log($"OnCompleted()");
            subject.OnCompleted();
        }
    }
}