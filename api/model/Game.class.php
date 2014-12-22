<?php
class Game extends Illuminate\Database\Eloquent\Model {
    protected $fillable = ['title', 'developer', 'publisher', 'year', 'image', 'finished', 'comment', 'sort_as', 'playtime', 'rating', 'currently_playing', 'queue_position'];
    protected $hidden = ['private_comment', 'genres', 'platforms', 'tags'];
    protected $appends = ['genre_ids', 'platform_ids', 'tag_ids'];
    
    public function genres() {
        return $this->belongsToMany('Genre');
    }
    
    public function platforms() {
        return $this->belongsToMany('Platform');
    }
    
    public function tags() {
        return $this->belongsToMany('Tag');
    }
    
    public function user() {
        return $this->belongsTo('User');
    }
    
    public function getGenreIdsAttribute() {
        $ids = [];
        foreach($this->genres as $genre)
            array_push($ids, $genre->id);
        
        return $ids;
    }
    
    public function getPlatformIdsAttribute() {
        $ids = [];
        foreach($this->platforms as $platform)
            array_push($ids, $platform->id);
        
        return $ids;
    }
    
    public function getTagIdsAttribute() {
        $ids = [];
        foreach($this->tags as $tag)
            array_push($ids, $tag->id);
        
        return $ids;
    }
    
    public function addImage($image) {
        if(!is_uploaded_file($image['tmp_name']))
            return false;

        $split = explode('.', $image['name']);
        $filename = 'image.' . $split[count($split) - 1]; // image.ext
        $dir = '../public/images/'.$this->id;
        if(file_exists($dir))
            deleteDirectoryRecursively($dir);
        mkdir($dir);
        
        $fullpath = $dir.'/'.$filename;
        
        move_uploaded_file($image['tmp_name'], $fullpath);

        $this->image = 'images/'.$this->id.'/'.$filename;
        $this->save();
        
        return true;
    }
    
    public function deleteImage() {
        if(!$this->id)
            return;
        
        deleteDirectoryRecursively('../public/images/'.$this->id);
    }
}
