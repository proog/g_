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

    public function getIdAttribute($value) {
        return (int) $value;
    }

    public function getViewAttribute($value) {
        return $value !== null ? (int) $value : null;
    }

    public function isValid()  {
        try {
            $this->validOrThrow();
        }
        catch(InvalidModelException $e) {
            return false;
        }

        return true;
    }

    public function validOrThrow() {
        if(!$this->username)
            throw new InvalidModelException('Invalid username');

        if(!$this->password || strlen($this->password) != 64)
            throw new InvalidModelException('Invalid password');
    }
}
