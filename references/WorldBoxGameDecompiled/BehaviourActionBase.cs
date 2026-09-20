using ai.behaviours;

public class BehaviourActionBase<T> : BehaviourElementAI
{
	public bool uses_kingdoms;

	public bool uses_cities;

	public bool uses_books;

	public bool uses_religions;

	public bool uses_languages;

	public bool uses_cultures;

	public bool uses_clans;

	public bool uses_plots;

	public bool uses_families;

	protected bool _has_error_check;

	protected static MapBox world => MapBox.instance;

	public override void create()
	{
		base.create();
		setupErrorChecks();
	}

	public virtual void prepare(T pObject)
	{
	}

	public virtual BehResult startExecute(T pObject)
	{
		if (_has_error_check)
		{
			if (shouldRetry(pObject))
			{
				return BehResult.RepeatStep;
			}
			if (errorsFound(pObject))
			{
				return BehResult.Stop;
			}
		}
		prepare(pObject);
		return execute(pObject);
	}

	public virtual BehResult execute(T pObject)
	{
		return BehResult.Continue;
	}

	protected virtual void setupErrorChecks()
	{
		setHasErrorCheck();
	}

	private void setHasErrorCheck()
	{
		_has_error_check = true;
	}

	public virtual bool errorsFound(T pObject)
	{
		return false;
	}

	public virtual bool shouldRetry(T pObject)
	{
		if (uses_cities && world.cities.isLocked())
		{
			return true;
		}
		if (uses_kingdoms && world.kingdoms.isLocked())
		{
			return true;
		}
		if (uses_books && world.books.isLocked())
		{
			return true;
		}
		if (uses_religions && world.religions.isLocked())
		{
			return true;
		}
		if (uses_languages && world.languages.isLocked())
		{
			return true;
		}
		if (uses_cultures && world.cultures.isLocked())
		{
			return true;
		}
		if (uses_clans && world.clans.isLocked())
		{
			return true;
		}
		if (uses_plots && world.plots.isLocked())
		{
			return true;
		}
		if (uses_families && world.families.isLocked())
		{
			return true;
		}
		return false;
	}
}
