# Things I want to do 

needs to be used with the unity agents as well.

this needs to be robust. it can take long time, can grow in scope, can have failures and all, but if It's marked as done it needs to stay done. removing it will not be tolerated. if it happens more than 3 times. I will drop this project

1. 
2.
3.


# Enemies

at least 80 enemies

## pathfinding 

### horizontal 

- **use agents to go to a path and using the base of the context system.**

- find player
- if sniper or something, find high ground 
- if heavy, look for enemy group and protect them. (maslow hierarchy of needs)
- if melee go find the shortest path to go to player
- sweet spots

### vertical

- behaviour to go to high spots

horizontal and vertical
  
context steering

# Grouping

- strong together
- could have a heatmap for position?
- Really nice feature. if player is cheeky and moving back and forth from one position to  another, would be really nice to at least one enemy to wait for that

# Enemy actions

enemy actions

enemy needs a kinda flow tree where it decides what to do or the hive does it

## enemy weights

horizontal movement weights

vertical movement weights

attack weights?

different paths weights

spawning weights?

maybe looking something like this

```c#
class EnemyAction {
    public Conditions[] conditions;
    public float defaultWeight;
    public float GetWeight(State state) {
        if (conditions.Length() == 0 ) {
            return defaultWeight;
        }
        float weight= 0;
        
        foreach (Condition cond in conditions) {
            weight += cond.Run(state);
        } 
    } 
}

class Condition {
    private List<Func<object,float>> fns;
    int Length() {
        return fns.Count;
    }
    
    public void Add(Func<object,float> fn) {
        fns.Add(fn)
    }
    public float Run(object state){
        float result = 0f;
        foreach (var fn of fns) {
            result += fn(state)
        }
        return result;
    }
}

```

group behaviour

# Customization 

- min max distance for enemy

enemy customization

api or menu?

think api is easier


# finals

now you can care about performance


# Testing

how can I make automated testing for that?

https://unity.com/how-to/automated-tests-unity-test-framework

at least 2 tests for every point in the list

everything needs to be marked as done only once, that is until reworking things for performance



# things that might help 

- now https://www.youtube.com/watch?v=Q1xZGt41N80
- now https://www.youtube.com/watch?v=m7VY1T6f8Ak

ai 
- chess AI for scoring https://www.youtube.com/watch?v=U4ogK0MIzqk&t=1s
- chess AI for scoring https://www.youtube.com/watch?v=_vqlIPDR2TU
- for grouping https://www.youtube.com/watch?v=bqtqltqcQhw
- Boids system  https://www.youtube.com/watch?v=r_It_X7v-1E
- movement https://www.youtube.com/watch?v=nWuekr5rUcg
- spawning system https://www.youtube.com/watch?v=vOC3usydLeE
- tile mapping (could use for other things) https://www.youtube.com/watch?v=gIUVRYViG_g
- spawning system could be useful to see https://www.youtube.com/watch?v=ajwRvAGKl_k
- context steering behaviour pdf https://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter18_Context_Steering_Behavior-Driven_Steering_at_the_Macro_Scale.pdf


## development

## Debugging

- I can press a mouse button on a position, and it makes the enemy go there
- I can visually see the action on the enemy/group of enemies (could be color coded)

- I can press a key combination to make selected enemies or something to execute an action

### user actions

keyboard or button on screen, whatever is easier

```c#
class DebugActor {
    public bool isActive = true;
    private Dictionary<string,Func<void, void>> actions;
    private void Start() {
        //still can setup even though is not active because in the middle of play session i might want to activate it, idk just something to think about
    }
    //doesnt exist just making it up
    OnKey(String key) {
        if (!isActive) return;
        if (actions.Contains(key)) {
            actions[key]();
        }
    }
}
```


- control A to select all enemies   
- control F to go to something or button on screen, whatever is easier


- debugging https://www.youtube.com/watch?v=tdISDcM1oxo
- more c# reference and value https://www.youtube.com/watch?v=XSUBp-EZhBE
- script communication https://www.youtube.com/watch?v=vDrYDAMdpuc
- object pooling performance https://www.youtube.com/watch?v=LhqP3EghQ-Q
- when performance matters look into https://chatgpt.com/c/674cc2b3-f568-8004-91bb-630b1c6032e5


## maths 

- point in triangle https://www.youtube.com/watch?v=HYAgJN3x4GA
- distance from point to line https://www.youtube.com/watch?v=KHuI9bXZS74





