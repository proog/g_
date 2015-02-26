<?php
abstract class CategoryEntity extends Illuminate\Database\Eloquent\Model {
    protected $fillable = ['name', 'short_name'];
    
    public function games() {
        return $this->belongsToMany('Game');
    }

    public function user() {
        return $this->belongsTo('User');
    }

    public function getIdAttribute($value) {
        return (int) $value;
    }

    public function getUserIdAttribute($value) {
        return (int) $value;
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
        if(!$this->name)
            throw new InvalidModelException('Invalid name');

        if(!$this->short_name)
            throw new InvalidModelException('Invalid short name');
    }
}
