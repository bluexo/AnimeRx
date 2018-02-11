﻿using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace AnimeRx
{
    public static partial class Anime
    {
        public static IObservable<Vector3> Play(Vector3 from, Vector3 to, IAnimator animator)
        {
            return Play(from, to, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> Play(Vector3 from, Vector3 to, IAnimator animator, IScheduler scheduler)
        {
            return PlayInternal(animator, Vector3.Distance(from, to), scheduler)
                .Select(x => Vector3.LerpUnclamped(from, to, x));
        }

        public static IObservable<Vector3> PlayIn(Vector3 from, Vector3 inEnd, Vector3 to, IAnimator inAnimator)
        {
            return PlayIn(from, inEnd, to, inAnimator, DefaultScheduler);
        }

        public static IObservable<Vector3> PlayIn(Vector3 from, Vector3 inEnd, Vector3 to, IAnimator inAnimator, IScheduler scheduler)
        {
            var velocity = inAnimator.CalcFinishVelocity(Vector3.Distance(inEnd, from));
            return Play(from, inEnd, inAnimator, scheduler)
                .Play(to, Easing.Linear(velocity), scheduler);
        }

        public static IObservable<Vector3> PlayOut(Vector3 from, Vector3 outStart, Vector3 to, IAnimator outAnimator)
        {
            return PlayOut(from, outStart, to, outAnimator, DefaultScheduler);
        }

        public static IObservable<Vector3> PlayOut(Vector3 from, Vector3 outStart, Vector3 to, IAnimator outAnimator, IScheduler scheduler)
        {
            var velocity = outAnimator.CalcStartVelocity(Vector3.Distance(to, outStart));
            return Play(from, outStart, Easing.Linear(velocity), scheduler)
                .Play(to, outAnimator);
        }

        public static IObservable<Vector3> PlayInOut(Vector3 from, Vector3 inEnd, Vector3 outStart, Vector3 to, IAnimator inAnimator, IAnimator outAnimator)
        {
            return PlayInOut(from, inEnd, outStart, to, inAnimator, outAnimator, DefaultScheduler);
        }

        public static IObservable<Vector3> PlayInOut(Vector3 from, Vector3 inEnd, Vector3 outStart, Vector3 to, IAnimator inAnimator, IAnimator outAnimator, IScheduler scheduler)
        {
            var inVelocity = inAnimator.CalcFinishVelocity(Vector3.Distance(inEnd, from));
            return Play(from, inEnd, inAnimator, scheduler)
                .Play(outStart, Easing.Linear(inVelocity), scheduler)
                .Play(to, outAnimator, scheduler);
        }

        public static IObservable<Vector3> Play(Vector3[] path, IAnimator animator)
        {
            return Play(path, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> Play(Vector3[] path, IAnimator animator, IScheduler scheduler)
        {
            var distance = new List<float>();
            var sum = 0.0f;
            for (var i = 0; i < path.Length - 1; ++i)
            {
                var d = Vector3.Distance(path[i], path[i + 1]);
                distance.Add(sum + d);
                sum += d;
            }
            return PlayInternal(animator, sum, scheduler)
                .Select(x =>
                {
                    var a = x * sum;
                    var i = 0;
                    for (; i < distance.Count - 1; i++)
                    {
                        if (distance[i] > a) break;
                    }

                    var b = i == 0 ? 0 : distance[i - 1];
                    return Vector3.LerpUnclamped(path[i], path[i + 1], (a - b) / (distance[i] - b));
                });
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3 from, Vector3 to, IAnimator animator)
        {
            return Play(self, from, to, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3 from, Vector3 to, IAnimator animator, IScheduler scheduler)
        {
            return self.Concat(Play(from, to, animator, scheduler));
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3 from, Vector3[] path, IAnimator animator)
        {
            return Play(self, from, path, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3 from, Vector3[] path, IAnimator animator, IScheduler scheduler)
        {
            var merged = new Vector3[path.Length + 1];
            merged[0] = from;
            Array.Copy(path, 0, merged, 1, path.Length);
            return self.Concat(Play(merged, animator, scheduler));
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3 to, IAnimator animator)
        {
            return Play(self, to, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3 to, IAnimator animator, IScheduler scheduler)
        {
            return self.Select(x => Observable.Return(x).Concat(Play(x, to, animator, scheduler))).Switch();
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3[] path, IAnimator animator)
        {
            return Play(self, path, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> Play(this IObservable<Vector3> self, Vector3[] path, IAnimator animator, IScheduler scheduler)
        {
            return self.Select(x =>
            {
                var merged = new Vector3[path.Length + 1];
                merged[0] = x;
                Array.Copy(path, 0, merged, 1, path.Length);
                return Observable.Return(x).Concat(Play(merged, animator, scheduler));
            }).Switch();
        }

        public static IObservable<Vector3> PlayRelative(Vector3 from, Vector3 relative, IAnimator animator)
        {
            return PlayRelative(from, relative, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> PlayRelative(Vector3 from, Vector3 relative, IAnimator animator, IScheduler scheduler)
        {
            return Play(from, from + relative, animator, scheduler);
        }

        public static IObservable<Vector3> PlayRelative(this IObservable<Vector3> self, Vector3 from, Vector3 relative, IAnimator animator)
        {
            return PlayRelative(self, from, relative, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> PlayRelative(this IObservable<Vector3> self, Vector3 from, Vector3 relative, IAnimator animator, IScheduler scheduler)
        {
            return self.Concat(Play(from, from + relative, animator, scheduler));
        }

        public static IObservable<Vector3> PlayRelative(this IObservable<Vector3> self, Vector3 relative, IAnimator animator)
        {
            return PlayRelative(self, relative, animator, DefaultScheduler);
        }

        public static IObservable<Vector3> PlayRelative(this IObservable<Vector3> self, Vector3 relative, IAnimator animator, IScheduler scheduler)
        {
            return self.Select(x => Observable.Return(x).Concat(Play(x, x + relative, animator, scheduler))).Switch();
        }

        public static IObservable<Vector3> Lerp(this IObservable<float> self, Vector3 from, Vector3 to)
        {
            return self.Select(x => Vector3.LerpUnclamped(from, to, x));
        }
    }
}
