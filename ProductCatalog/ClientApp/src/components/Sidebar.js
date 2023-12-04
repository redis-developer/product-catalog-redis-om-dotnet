import React, {useState} from 'react';
import {Button} from "reactstrap";
import FacetSelector from "./FacetSelector";

const Sidebar = ({updateFilter, filterOptions}) =>{   

    const updateFilterItems = async (fieldName, items) =>{
        await updateFilter(fieldName, items);
    }
    
    return(
        <div style={{width: "20%", paddingRight: 10}} className="sidebar">            
            <FacetSelector updateFilterCallback={updateFilterItems} facetName="Categories" facetItems={filterOptions.categories}/>            
            <FacetSelector updateFilterCallback={updateFilterItems} facetName="Years" facetItems={filterOptions.years}/>
            <FacetSelector updateFilterCallback={updateFilterItems} facetName="Seasons" facetItems={filterOptions.seasons}/>
            <FacetSelector updateFilterCallback={updateFilterItems} facetName="Genders" facetItems={filterOptions.genders}/>
            <FacetSelector updateFilterCallback={updateFilterItems} facetName="Usages" facetItems={filterOptions.usages}/>
            <FacetSelector updateFilterCallback={updateFilterItems} facetName="Colors" facetItems={filterOptions.colors}/>            
        </div>
    )
}

export default Sidebar;