var entity;
//var Components = importNamespace("Escape.Components")

import { Transform3D } from 'enginelib';

export function init(e) {
    entity = e;
}

export function deinit(e) {
    entity = null;
}

export function update(delta) {
    message("I am " + entity.Id)
    
    message(entity.Get(Transform3D))
    //getComponent(entity, Components.Transform3D).Translate(0.01, 0, 0)
}

export function render(delta, o) {}
