<?php
class User extends Illuminate\Database\Eloquent\Model {
    protected $guarded = ['id', 'username'];
    protected $hidden = ['password'];
    
    public function games() {
        return $this->hasMany('Game');
    }

    public function genres() {
        return $this->hasMany('Genre');
    }

    public function platforms() {
        return $this->hasMany('Platform');
    }

    public function tags() {
        return $this->hasMany('Tag');
    }
}
